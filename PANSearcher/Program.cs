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
        ///     Paths to exclude from search.
        /// </summary>
        [Argument('x', "exclude", "Paths to exclude from search.")]
        private static string[]? ExcludedPaths { get; set; }

        /// <summary>
        ///     Text file extensions to search.
        /// </summary>
        [Argument('t', "textfiles", "Text file extensions to search.")]
        private static string[]? TextFileExtensions { get; set; }

        /// <summary>
        ///     Displays help text and exits.
        /// </summary>
        [Argument('h', "help", "Displays help text and exits.")]
        private static bool ShowHelpText { get; set; }

        /// <summary>
        ///     Displays PAN numbers unmasked. Ignored when used with 't' flag.
        /// </summary>
        [Argument('u', "unmask", "Displays PAN numbers unmasked. Ignored when used with 't' flag.")]
        private static bool Unmask { get; set; }

        /// <summary>
        ///     Displays PAN numbers truncated.
        /// </summary>
        [Argument('t', "truncate", "Displays PAN numbers truncated.")]
        private static bool Truncate { get; set; }

        /// <summary>
        ///     Quiet mode.
        /// </summary>
        [Argument('q', "quiet", "Quiet mode.")]
        private static bool Quiet { get; set; }

        /// <summary>
        ///     Verbose output. Ignored when used with 'q' flag.
        /// </summary>
        [Argument('v', "verbose", "Verbose output. Ignored when used with 'q' flag.")]
        private static bool Verbose { get; set; }

        /// <summary>
        ///     Path of configuration file.
        /// </summary>
        [Argument('c', "config", "Path of configuration file.")]
        private static string? ConfigFile { get; set; }

        /// <summary>
        ///     Displays help text.
        /// </summary>
        [Argument('o', "outfile", "Output file name for PAN report.")]
        private static string? OutFile { get; set; }

        private static Report report;

        public static void Main(string[] args)
        {
            // enable ctrl+c
            Console.CancelKeyPress += (o, e) =>
            {
                Print.Output($"{Environment.NewLine}Operation cancelled by user.");
                Environment.Exit(1);
            };

            Arguments.Populate();

            if (ShowHelpText)
            {
                ShowHelp();
                return;
            }

            LoadSettings();

            Print.Output($"Started PAN number search. Root path: {SearchBase}{Environment.NewLine}");
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var findings = Search();
            stopwatch.Stop();
            Print.Output($"{Environment.NewLine}PAN search completed in {stopwatch.Elapsed}.");

            if (findings.Count == 0)
            {
                Print.Output($"{Environment.NewLine}No files with PAN number found.");
            }
            else
            {
                Print.Output($"{Environment.NewLine}Total {findings.Count} files found with at least one PAN number. To ignore the false positives, you can configure to ignore those folders.");
            }

            PrintReport(findings, string.Join(' ', args));
        }

        private static void PrintReport(List<Finding> findings, string args)
        {
            report = new Report(findings);
#pragma warning disable CS8604 // Possible null reference argument.
            var command = Path.Combine(Process.GetCurrentProcess().ProcessName, args);
            var reportText = report.Prepare(SearchBase, string.Join(' ', ExcludedPaths), command);
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var path = Path.Combine(home, "PANSearcher");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            File.WriteAllText(Path.Combine(path, OutFile.Replace("%s", DateTime.Now.ToString("yyyy-MM-dd-HHmmss"))), reportText);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.
        }

        private static void LoadSettings()
        {
            // Not all settings are configured
            Print.PrintMode = GetPrintMode();


            // First load the application defaults or given config file
            // Unless specified, config values will be used
            Settings.Instance.LoadFromFile(ConfigFile);

            if (string.IsNullOrEmpty(SearchBase))
            {
                SearchBase = Settings.Instance.SearchBase;
            }

            if (TextFileExtensions == null)
            {
                TextFileExtensions = Settings.Instance.TextFileExtensions as string[];
            }

            if (ExcludedPaths == null)
            {
                ExcludedPaths = Settings.Instance.ExcludeFolders as string[];
            }

            if (OutFile == null)
            {
                OutFile = Settings.Instance.OutputFileName;
            }
        }

        private static PrintMode GetPrintMode()
        {
            if (Quiet)
            {
                return PrintMode.Quiet;
            }
            else if (Verbose)
            {
                return PrintMode.Verbose;
            }
            else
            {
                return PrintMode.Output;
            }
        }

        private static List<Finding> Search()
        {
            var engine = new SearchEngine();
            var findings = new List<Finding>();

#pragma warning disable CS8604 // Possible null reference argument.
            // TODO: A task for each type of files.
            var tasks = new Task<List<Finding>>[1];
            tasks[0] = new Task<List<Finding>>(() => SearchEngine.Search(SearchBase, ExcludedPaths, TextFileExtensions, new TextFileContext(), DisplayMode));
            tasks[0].Start();
#pragma warning restore CS8604 // Possible null reference argument.

            Task.WaitAll(tasks);
            foreach (var task in tasks)
            {
                findings.AddRange(task.Result);
            }
            return findings;
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

            Print.Output($"Short\tLong\t\t{"Type".PadRight(maxLen)}\tFunction");
            Print.Output($"-----\t----\t\t{"----".PadRight(maxLen)}\t--------");

            foreach (var item in helpAttributes)
            {
                var result = item.ShortName + "\t" + item.LongName + "\t" + item.Property.PropertyType.ToColloquialString().PadRight(maxLen) + "\t" + item.HelpText;
                Print.Output(result);
            }
        }
    }
}