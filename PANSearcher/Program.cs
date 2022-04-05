using System.Diagnostics;
using Utility.CommandLine;

namespace PANSearcher
{
    public static class Program
    {
        /// <summary>
        ///     Gets or sets the base search directory.
        /// </summary>
        [Argument('s', "search", @"Base directory to search in (default: C:\)")]
        private static string? SearchBase { get; set; }


        /// <summary>
        ///     Displays help text.
        /// </summary>
        [Argument('h', "selp", "Displays help text and exits.")]
        private static bool ShowHelpText { get; set; }

        /// <summary>
        ///     Displays help text.
        /// </summary>
        [Argument('u', "unmask", "Displays PAN numbers unmasked. Ignored when used with 'u' flag. (Default: false)")]
        private static bool Unmask { get; set; }

        /// <summary>
        ///     Displays help text.
        /// </summary>
        [Argument('t', "truncate", "Displays PAN numbers truncated.")]
        private static bool Truncate { get; set; }

        /// <summary>
        ///     Displays help text.
        /// </summary>
        [Argument('c', "config", "configuration file to use")]
        private static string? ConfigFile { get; set; }

        private static Config? config;

        public static void Main(string[] args)
        {
            // enable ctrl+c
            Console.CancelKeyPress += (o, e) =>
            {
                Console.WriteLine("Operation cancelled by user.");
                Environment.Exit(1);
            };

            Arguments.Populate();

            if (ShowHelpText)
            {
                ShowHelp();
                return;
            }

            // First load the application defaults or given config file
            // Unless specified, config values will be used
            if (!string.IsNullOrEmpty(ConfigFile))
            {
                config = new Config(ConfigFile);
            }
            else
            {
                config = new Config();
            }

            if (string.IsNullOrEmpty(SearchBase))
            {
                SearchBase = config.SearchBase;
            }

            var files = new List<string>();

            var options = new EnumerationOptions
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = true
            };

            // TODO: More foreach for each type of extensions.
#pragma warning disable CS8604 // Possible null reference argument.
            foreach (var textFileExt in config.TextFileExtensions.ToList())
            {
                files.AddRange(Directory.EnumerateFiles(SearchBase, $"*{textFileExt}", options));
            }
#pragma warning restore CS8604 // Possible null reference argument.

            Console.WriteLine($"Found {files.Count} text file(s) under {SearchBase}{Environment.NewLine}");

            var fileCounter = 0;
            foreach (var file in files)
            {
                IEnumerable<string>? lines = null;

                try
                {
                    lines = File.ReadLines(file, System.Text.Encoding.UTF8);
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
                            string pan;
                            if (Truncate) { pan = PAN.Truncate(item); }
                            else if (Unmask) { pan = item; }
                            else { pan = PAN.Mask(item); }
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
            if (fileCounter == 0)
            {
                Console.WriteLine($"{Environment.NewLine}No files with PAN number found.");
            }
            else
            {
                Console.WriteLine($"{Environment.NewLine}Total {fileCounter} files found with at leas one PAN number. To ignore the false positives, you can configure to ignore those folders.");
            }
        }

        /// <summary>
        ///     Returns a "pretty" string representation of the provided Type; specifically, corrects the naming of generic Types
        ///     and appends the type parameters for the type to the name as it appears in the code editor.
        /// </summary>
        /// <param name="type">The type for which the colloquial name should be created.</param>
        /// <returns>A "pretty" string representation of the provided Type.</returns>
        public static string ToColloquialString(this Type type) => (!type.IsGenericType ? type.Name : type.Name.Split('`')[0] + "<" + string.Join(", ", type.GetGenericArguments().Select(a => a.ToColloquialString())) + ">");


        /// <summary>
        ///     Show help for arguments.
        /// </summary>
        private static void ShowHelp()
        {
            var helpAttributes = Arguments.GetArgumentInfo(typeof(Program));

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var maxLen = helpAttributes.Select(a => a.Property.PropertyType.ToColloquialString()).OrderByDescending(s => s.Length).FirstOrDefault().Length;
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            Console.WriteLine($"Short\tLong\t\t{"Type".PadRight(maxLen)}\tFunction");
            Console.WriteLine($"-----\t----\t\t{"----".PadRight(maxLen)}\t--------");

            foreach (var item in helpAttributes)
            {
                var result = item.ShortName + "\t" + item.LongName + "\t" + item.Property.PropertyType.ToColloquialString().PadRight(maxLen) + "\t" + item.HelpText;
                Console.WriteLine(result);
            }
        }
    }
}