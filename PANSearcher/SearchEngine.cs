﻿namespace PANSearcher.Context
{
    public class SearchEngine
    {
        public static Report Search(string searchBase, string[] excluded, string[] extensions, IContext context, PANDisplayMode displayMode = PANDisplayMode.Masked)
        {
            var report = new Report();

            var tasks = new Task<Report>[extensions.Length];
            for (var i = 0; i < extensions.Length; i++)
            {
                var extension = extensions[i];
                tasks[i] = new Task<Report>(() => RunSearch(searchBase, excluded, context, displayMode, extension));
                tasks[i].Start();
            }
            _ = Task.WhenAll(tasks);
            foreach (var task  in tasks)
            {
                report.ImportFrom(task.Result);
            }

            return report;
        }

        private static Report RunSearch(string searchBase, string[] excluded, IContext context, PANDisplayMode displayMode, string textFileExt)
        {
            var findings = new List<Finding>();

            Print.Verbose($"Started searching for files with '*{textFileExt}' extensions");
            var options = new EnumerationOptions
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = true
            };
            var files = Directory.EnumerateFiles(searchBase, $"*{textFileExt}", options).Where(f => !IsExcluded(f, excluded));

            var matches = from file in files.AsParallel().AsOrdered().WithMergeOptions(ParallelMergeOptions.NotBuffered)
                          from line in context.Read(file).Zip(Enumerable.Range(1, int.MaxValue), (s, i) => new { Num = i, Text = s, File = file })
                          from found in PAN.ParseLine(line.Text)
                          where (found.Any() && PAN.Validate(found, out _))
                          select new { line, found };

            foreach (var match in matches)
            {
                _ = PAN.Validate(match.found, out var cardType);
                var pan = PAN.Format(match.found, displayMode);
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
