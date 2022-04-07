namespace PANSearcher.Context
{
    public class SearchEngine
    {
        public static void Search(string searchBase, string[] excluded, string[] extensions, IContext context, PANDisplayMode displayMode = PANDisplayMode.Masked)
        {
            var fileCounter = 0;
            var tasks = new Task<int>[extensions.Length];
            for (var i = 0; i < extensions.Length; i++)
            {
                var extension = extensions[i];
                tasks[i] = new Task<int>(() => RunSearch(searchBase, excluded, context, displayMode, extension));
                tasks[i].Start();
            }
            foreach (var task in tasks)
            {
                fileCounter += task.Result;
            }

            if (fileCounter == 0)
            {
                Print.Output($"{Environment.NewLine}No files with PAN number found.");
            }
            else
            {
                Print.Output($"{Environment.NewLine}Total {fileCounter} files found with at least one PAN number. To ignore the false positives, you can configure to ignore those folders.");
            }
        }

        private static int RunSearch(string searchBase, string[] excluded, IContext context, PANDisplayMode displayMode, string textFileExt)
        {
            Print.Output($"Started searching for files with '*{textFileExt}' extensions");
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
            }

            return matches.GroupBy(m => m.line.File).Count();
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
