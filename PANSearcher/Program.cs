using System.Diagnostics;
using PANSearcher.Context;
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
        [Argument('x', "exclude", "Paths to exclude from search.")]
        private static string[]? ExcludedPaths { get; set; }

        /// <summary>
        ///     Displays help text.
        /// </summary>
        [Argument('h', "help", "Displays help text and exits.")]
        private static bool ShowHelpText { get; set; }

        /// <summary>
        ///     Displays help text.
        /// </summary>
        [Argument('u', "unmask", "Displays PAN numbers unmasked. Ignored when used with 't' flag.")]
        private static bool Unmask { get; set; }

        /// <summary>
        ///     Displays help text.
        /// </summary>
        [Argument('t', "truncate", "Displays PAN numbers truncated.")]
        private static bool Truncate { get; set; }

        /// <summary>
        ///     Displays help text.
        /// </summary>
        [Argument('c', "config", "Configuration file to use")]
        private static string? ConfigFile { get; set; }

        public static void Main(string[] args)
        {
            // enable ctrl+c
            Console.CancelKeyPress += (o, e) =>
            {
                Console.WriteLine($"{Environment.NewLine}Operation cancelled by user.");
                Environment.Exit(1);
            };

            Arguments.Populate();

            if (ShowHelpText)
            {
                ShowHelp();
                return;
            }

            LoadSettings();

            Console.WriteLine($"Started PAN number search. Root path: {SearchBase}{Environment.NewLine}");
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            Search();
            stopwatch.Stop();
            Console.WriteLine($"{Environment.NewLine}PAN search completed in {stopwatch.Elapsed}.");
        }

        private static void LoadSettings()
        {
            // First load the application defaults or given config file
            // Unless specified, config values will be used
            Settings.Instance.LoadFromFile(ConfigFile);

            if (string.IsNullOrEmpty(SearchBase))
            {
                SearchBase = Settings.Instance.SearchBase;
            }

            if (ExcludedPaths == null)
            {
                ExcludedPaths = Settings.Instance.ExcludeFolders as string[];
            }
        }

        private static void Search()
        {
            var factory = new TaskFactory();
            var engine = new SearchEngine();

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            // TODO: A task for each type of files.
            var textFileContextTask = factory.StartNew(() => SearchEngine.Search(SearchBase, ExcludedPaths, Settings.Instance.TextFileExtensions, new TextFileContext(), DisplayMode));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.

            Task.WaitAll(textFileContextTask);
        }

        private static PANDisplayMode DisplayMode
        {
            get
            {
                if (Truncate)
                {
                    return PANDisplayMode.Truncated;
                }
                else if (Unmask)
                {
                    return PANDisplayMode.Unmasked;
                }
                else
                {
                    return PANDisplayMode.Masked;
                }
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