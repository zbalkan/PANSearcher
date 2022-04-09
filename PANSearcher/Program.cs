using System.Diagnostics;
using PANSearcher.Context;
using Utility.CommandLine;

namespace PANSearcher
{
    public static class Program
    {
        #region Arguments

        /// <summary>
        ///     Displays PAN numbers truncated. The flag 't' is used for text files, so a is used temporarily.
        /// </summary>
        [Argument('a', "truncate", "Displays PAN numbers truncated.")]
        private static bool Truncate { get; set; }

        /// <summary>
        ///     Path of configuration file.
        /// </summary>
        [Argument('c', "config", "Path of configuration file.")]
        private static string? ConfigFile { get; set; }

        /// <summary>
        ///     Displays help text and exits.
        /// </summary>
        [Argument('h', "help", "Displays help text and exits.")]
        private static bool ShowHelpText { get; set; }

        /// <summary>
        ///     Displays help text.
        /// </summary>
        [Argument('o', "outfile", "Output file name for PAN report.")]
        private static string? OutFile { get; set; }

        /// <summary>
        ///     Quiet mode.
        /// </summary>
        [Argument('q', "quiet", "Quiet mode.")]
        private static bool Quiet { get; set; }

        /// <summary>
        ///     Gets or sets the base search directory.
        /// </summary>
        [Argument('s', "search", @"Base directory to search in (default: C:\)")]
        private static string? SearchBase { get; set; }

        /// <summary>
        ///     Text file extensions to search.
        /// </summary>
        [Argument('t', "textfiles", "Text file extensions to search.")]
        private static string[]? TextFileExtensions { get; set; }

        /// <summary>
        ///     Displays PAN numbers unmasked. Ignored when used with 't' flag.
        /// </summary>
        [Argument('u', "unmask", "Displays PAN numbers unmasked. Ignored when used with 't' flag.")]
        private static bool Unmask { get; set; }

        /// <summary>
        ///     Verbose output. Ignored when used with 'q' flag.
        /// </summary>
        [Argument('v', "verbose", "Verbose output. Ignored when used with 'q' flag.")]
        private static bool Verbose { get; set; }

        /// <summary>
        ///     Paths to exclude from search.
        /// </summary>
        [Argument('x', "exclude", "Paths to exclude from search.")]
        private static string[]? ExcludedPaths { get; set; }
        #endregion Arguments

        private const string reportsFolderName = "PANSearcherReports";

        private static string? latestReport;

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

            Print.Output($"Started PAN number search. Root path: {Settings.Instance.SearchBase}{Environment.NewLine}");
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var report = Search();
            stopwatch.Stop();
            Print.Output($"{Environment.NewLine}PAN search completed in {stopwatch.Elapsed}.");

            if (report.Findings.Count == 0)
            {
                Print.Output($"{Environment.NewLine}No files with PAN number found.");
            }
            else
            {
#pragma warning disable IDE0071 // Simplify interpolation
                Print.Output($"{Environment.NewLine}Searched {report.NumberOfFiles.ToString()} files. Total {report.Findings.Count} files found with at least one PAN number. To ignore the false positives, you can configure to ignore those folders.");
#pragma warning restore IDE0071 // Simplify interpolation
            }

            PrintReport(report);
            Print.Output($"{Environment.NewLine}You can find the report: {latestReport}");
        }

        private static void PrintReport(Report report)
        {
#pragma warning disable CS8604 // Possible null reference argument.
            var reportText = report.Prepare();
            var path = Path.Combine((string?)Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), reportsFolderName);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            latestReport = Path.Combine(path, Settings.Instance.OutputFileName.Replace("%s", DateTime.Now.ToString("yyyy-MM-dd-HHmmss")));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            File.WriteAllText(latestReport, reportText);
#pragma warning restore CS8604 // Possible null reference argument.
        }

        private static void LoadSettings()
        {
            // First load the application defaults or given config file
            // Unless specified, config values will be used
            Settings.Instance.LoadFromFile(ConfigFile);

            if (!string.IsNullOrEmpty(SearchBase))
            {
                Settings.Instance.SearchBase = SearchBase;
            }

            if (TextFileExtensions != null)
            {
                Settings.Instance.TextFileExtensions = TextFileExtensions;
            }

            if (ExcludedPaths != null)
            {
                Settings.Instance.ExcludeFolders = ExcludedPaths;
            }

            if (OutFile != null)
            {
                Settings.Instance.OutputFileName = OutFile;
            }

            Settings.Instance.SetPrintMode(Quiet, Verbose);

            Settings.Instance.SetDisplayMode(Truncate, Unmask);
        }

        private static Report Search()
        {
            var engine = new SearchEngine();
            var report = new Report();

            // TODO: A task for each type of files.
            var tasks = new Task<Report>[2];
            tasks[0] = new Task<Report>(() => SearchEngine.Search(new TextFileContext()));
            tasks[1] = new Task<Report>(() => SearchEngine.Search(new ZipFileContext()));

            // Start all tasks and wait
            for (var i = 0; i < tasks.Length; i++)
            {
                var task = tasks[i];
                task.Start();
            }
            Task.WaitAll(tasks);

            // Combine reports
            for (var i = 0; i < tasks.Length; i++)
            {
                var task = tasks[i];
                report.ImportFrom(task.Result);
            }
            return report;
        }

        /// <summary>
        ///     Returns a "pretty" string representation of the provided Type; specifically, corrects the naming of generic Types
        ///     and appends the type parameters for the type to the name as it appears in the code editor.
        /// </summary>
        /// <param name="type">The type for which the colloquial name should be created.</param>
        /// <returns>A "pretty" string representation of the provided Type.</returns>
        private static string ToColloquialString(this Type type) => (!type.IsGenericType ? type.Name : type.Name.Split('`')[0] + "<" + string.Join(", ", type.GetGenericArguments().Select(a => a.ToColloquialString())) + ">");

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