using System.Diagnostics;

namespace PANSearcher.Context
{
    public class SearchEngine
    {
        public static void Search(string searchBase, IEnumerable<string> excluded, IEnumerable<string> extensions, IContext context, DisplayType displayType = DisplayType.Masked)
        {
            var options = new EnumerationOptions
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = true
            };
            var fileCounter = 0;

            foreach (var textFileExt in extensions)
            {
                Console.WriteLine($"Started searching for files with '*{textFileExt}' extensions");
                foreach (var file in Directory.EnumerateFiles(searchBase, $"*{textFileExt}", options).Where(f => !IsExcluded(f, excluded)))
                {
                    var increment = false; // reset file counter flag

                    IEnumerable<string>? lines = null;

                    try
                    {
                        lines = context.Read(file);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        continue;
                    }

                    if (lines == null || !lines.Any())
                    {
                        continue;
                    }

                    foreach (var line in lines)
                    {
                        var found = PAN.ParseLine(line);
                        if (found.Count == 0)
                        {
                            continue;
                        }

                        foreach (var item in found)
                        {
                            if (PAN.Validate(item, out var cardType))
                            {
                                var pan = PAN.Format(item, displayType);
                                Console.WriteLine($"FOUND PAN: {pan} - {Enum.GetName(typeof(CardType), cardType)} (Path: {file})");
                                increment = true;
                            }
                        }
                    }
                    if (increment)
                    {
                        fileCounter++;
                    }
                }
            }

            if (fileCounter == 0)
            {
                Console.WriteLine($"{Environment.NewLine}No files with PAN number found.");
            }
            else
            {
                Console.WriteLine($"{Environment.NewLine}Total {fileCounter} files found with at least one PAN number. To ignore the false positives, you can configure to ignore those folders.");
            }
        }

        private static bool IsExcluded(string filePath, IEnumerable<string> excluded)
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
