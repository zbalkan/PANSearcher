using PANSearcher.Context;

namespace PANSearcher
{
    public class SearchEngine
    {
        private static readonly EnumerationOptions defaultEnumOptions = new()
        {
            IgnoreInaccessible = true,
            RecurseSubdirectories = false,
            ReturnSpecialDirectories = false
        };

        public static Report Search(IContext context)
        {
            var extensions = Settings.Instance.FindExtensionsByContext(context);

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

            var report = new Report();
            for (var i = 0; i < tasks.Length; i++)
            {
                var task = tasks[i];
                report.ImportFrom(task.Result);
            }

            return report;
        }

        private static Report RunSearch(IContext context, string extension)
        {
            var searchBase = Settings.Instance.SearchBase;
            Print.Verbose($"Started searching for files with '*{extension}' extensions");

#pragma warning disable CS8604 // Possible null reference argument.
            var files = SearchFiles(searchBase, $"*{extension}");
#pragma warning restore CS8604 // Possible null reference argument.

            Print.Verbose($"Finished searching for files with '*{extension}' extensions");

            return SearchInFiles(context, files);
        }

        /// <summary> 
        /// A safe way to get all the files in a directory and sub directory without crashing on UnauthorizedException or PathTooLongException 
        /// </summary> 
        /// <param name="path">Starting directory</param> 
        /// <param name="pattern">Filename pattern</param> 
        /// <returns>List of files</returns> 
        private static List<string> SearchFiles(string path, string pattern)
        {
            var files = new List<string>();

            if (!IsExcluded(path))
            {
                try
                {
                    foreach (var directory in Directory.EnumerateDirectories(path))
                    {
                        files.AddRange(SearchFiles(directory, pattern));
                    }
                }
                catch
                {
                    // Skip those directories
                }

                try
                {
                    files.AddRange(Directory.EnumerateFiles(path, pattern, defaultEnumOptions));
                }
                catch
                {
                    // Skip those files
                }
            }

            return files;
        }
        private static bool IsExcluded(string filePath)
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            for (var i = 0; i < Settings.Instance.ExcludeFolders.Length; i++)
            {
                if (filePath.StartsWith(Settings.Instance.ExcludeFolders[i], StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            return false;
        }

        private static Report SearchInFiles(IContext context, List<string> files)
        {
            var findings = new List<Finding>();

            var matches = from file in files.AsParallel().AsOrdered().WithMergeOptions(ParallelMergeOptions.NotBuffered)
                          from line in context.Read(file).Zip(Enumerable.Range(1, int.MaxValue), (s, i) => new { Num = i, Text = s, File = file })
                          from found in PAN.ParseLine(line.Text)
                          where (found.Any() && PAN.Validate(found, out _))
                          select new { line, found };

            foreach (var match in matches)
            {
                _ = PAN.Validate(match.found, out var cardType);
                var pan = PAN.Format(match.found, Settings.Instance.PANDisplayMode);
                Print.Output($"FOUND PAN: {pan} - {Enum.GetName(typeof(CardType), cardType)} [{match.line.File} : {match.line.Num}]");

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
                NumberOfFiles = files.Count
            };
            return report;
        }
    }
}
