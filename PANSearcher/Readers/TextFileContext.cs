using System.Text;
using System.Diagnostics;

namespace PANSearcher.Readers
{
    public class TextFileContext
    {
        private readonly List<string> _extensions;

        public TextFileContext(IEnumerable<string> extensions)
        {
            if (extensions is null)
            {
                throw new ArgumentNullException(nameof(extensions));
            }

            _extensions = extensions.ToList();
        }

        public void Search(string searchBase, DisplayType displayType = DisplayType.Masked)
        {
            var options = new EnumerationOptions
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = true
            };
            var fileCounter = 0;

            foreach (var textFileExt in _extensions)
            {
                foreach (var file in Directory.EnumerateFiles(searchBase, $"*{textFileExt}", options))
                {
                    IEnumerable<string>? lines = null;

                    try
                    {
                        lines = File.ReadLines(file, Encoding.UTF8);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        continue;
                    }

                    var increment = false;
                    foreach (var line in lines)
                    {
                        var found = PAN.ParseLine(line);
                        if (found.Count == 0)
                        {
                            continue;
                        }

                        foreach (var item in found)
                        {
                            var cardType = PAN.GetCardType(item);
                            if (cardType == CardType.Invalid)
                            {
                                continue;
                            }

                            if (cardType != CardType.Invalid && Luhn.Validate(item))
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
    }
}
