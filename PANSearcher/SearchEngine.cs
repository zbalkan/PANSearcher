namespace PANSearcher.Context
{
    public class SearchEngine
    {
        public static Report Search(IContext context)
        {

            var extensions = Settings.Instance.TextFileExtensions as string[];
            var report = new Report();

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var tasks = new Task<Report>[extensions.Length];
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            for (var i = 0; i < extensions.Length; i++)
            {
                var extension = extensions[i];
                tasks[i] = new Task<Report>(() => RunSearch(context, extension));
                tasks[i].Start();
            }
            _ = Task.WhenAll(tasks);
            foreach (var task  in tasks)
            {
                report.ImportFrom(task.Result);
            }

            return report;
        }

        private static Report RunSearch(IContext context, string extension)
        {
            var searchBase = Settings.Instance.SearchBase;
            var excluded = Settings.Instance.ExcludeFolders as string[];
            var findings = new List<Finding>();

            Print.Verbose($"Started searching for files with '*{extension}' extensions");
            var options = new EnumerationOptions
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = true
            };
#pragma warning disable CS8604 // Possible null reference argument.
            var files = Directory.EnumerateFiles(searchBase, $"*{extension}", options).Where(f => !IsExcluded(f, excluded));
#pragma warning restore CS8604 // Possible null reference argument.

            var matches = from file in files.AsParallel().AsOrdered().WithMergeOptions(ParallelMergeOptions.NotBuffered)
                          from line in context.Read(file).Zip(Enumerable.Range(1, int.MaxValue), (s, i) => new { Num = i, Text = s, File = file })
                          from found in PAN.ParseLine(line.Text)
                          where (found.Any() && PAN.Validate(found, out _))
                          select new { line, found };

            foreach (var match in matches)
            {
                _ = PAN.Validate(match.found, out var cardType);
                var pan = PAN.Format(match.found, Settings.Instance.PANDisplayMode);
                Print.Output($"FOUND PAN: {match.found} - {Enum.GetName(typeof(CardType), cardType)} [{match.line.File} : {match.line.Num}]");

                var finding = findings.FirstOrDefault(f => f.FilePath.Equals(match.line.File, StringComparison.OrdinalIgnoreCase));
                if (finding == null)
                {
                    findings.Add(new Finding()
                    {
                        FilePath = match.line.File,
                        Records = new List<Record>()
                        {
                            new Record()
                            {
                                PossiblePAN = match.found,
                                CardType = cardType,
                                LineNumber = match.line.Num,
                                LineText = match.line.Text
                            }
                        }
                    });
                }
                else
                {
                    finding.Records.Add(new Record()
                    {
                        PossiblePAN = match.found,
                        CardType = cardType,
                        LineNumber = match.line.Num,
                        LineText = match.line.Text
                    });
                }
            }
            var report = new Report(findings)
            {
                NumberOfFiles = files.Count()
            };
            return report;
        }

        private static bool IsExcluded(string filePath, string[] excluded)
        {
            foreach (var excludedPath in excluded)
            {
                if (filePath.StartsWith(excludedPath))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
