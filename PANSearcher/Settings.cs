using System.Runtime.InteropServices;
using PANSearcher.Context;
using SharpConfig;

namespace PANSearcher
{
    internal class Settings
    {
        public string? SearchBase { get; set; }

        public string[]? ExcludeFolders { get; set; }

        public string[]? TextFileExtensions { get; set; }

        public string[]? ZipFileExtensions { get; set; }

        public string[]? SpecialFileExtensions { get; set; }

        public string[]? MailFileExtensions { get; set; }

        public string[]? OtherFileExtensions { get; set; }

        public string? OutputFileName { get; set; }

        public bool Unmask { get; set; }

        public PrintMode PrintMode { get; set; } = PrintMode.Output;

        public PANDisplayMode PANDisplayMode { get; set; } = PANDisplayMode.Masked;

        public string[]? ExcludePans { get; set; }

        private static readonly Lazy<Settings> LazyInstance = new(() => new Settings());

        public static Settings Instance = LazyInstance.Value;

        private Section? _defaultSection;

        public void LoadFromFile(string? path = null)
        {
            Configuration config;
            if (string.IsNullOrEmpty(path))
            {
                config = new Configuration
                {
                    new Section("Default")
                };
            }
            else
            {
                config = Configuration.LoadFromFile(path);
            }

            _defaultSection = config["Default"];
            Map();
        }

        public void SetPrintMode(bool quiet, bool verbose)
        {
            if (quiet)
            {
                PrintMode =  PrintMode.Quiet;
            }
            else if (verbose)
            {
                PrintMode = PrintMode.Verbose;
            }
            else
            {
                PrintMode = PrintMode.Output;
            }
        }

        public void SetDisplayMode(bool truncate, bool unmask)
        {
            if (truncate)
            {
                PANDisplayMode = PANDisplayMode.Truncated;
            }
            else if (unmask)
            {
                PANDisplayMode = PANDisplayMode.Unmasked;
            }
            else
            {
                PANDisplayMode = PANDisplayMode.Masked;
            }
        }

        public string[]? FindExtensionsByContext(IContext context) => context switch
        {
            TextFileContext => TextFileExtensions,
            ZipFileContext => ZipFileExtensions,
            MailFileContext => MailFileExtensions,
            SpecialFileContext => SpecialFileExtensions,
            OtherFileContext => OtherFileExtensions,
            _ => null,
        };

        private void Map()
        {
            if (_defaultSection == null)
            {
                throw new InvalidOperationException("Invalid configuration.");
            }

            var searchBase = _defaultSection["search"].StringValue;
            SearchBase = string.IsNullOrEmpty(searchBase) ? FindSearchBaseByOS() : searchBase;

            var excludeFolders = _defaultSection["exclude"].StringValue;
            ExcludeFolders = string.IsNullOrEmpty(excludeFolders) ? FindExcludeFoldersByOS() : excludeFolders.Split(',');

            var textFileExtensions = _defaultSection["textfiles"].StringValue;
            TextFileExtensions = string.IsNullOrEmpty(textFileExtensions)
                ? (new string[] { ".doc", ".xls", ".xml", ".txt", ".csv", ".log" })
                : textFileExtensions.Split(',');

            var zipFileExtensions = _defaultSection["zipfiles"].StringValue;
            ZipFileExtensions = string.IsNullOrEmpty(zipFileExtensions) ? (new string[] { ".docx", ".xlsx", ".zip" }) : zipFileExtensions.Split(',');

            var specialFileExtensions = _defaultSection["specialfiles"].StringValue;
            SpecialFileExtensions = string.IsNullOrEmpty(zipFileExtensions) ? (new string[] { ".msg" }) : specialFileExtensions.Split(',');

            var mailFileExtensions = _defaultSection["mailfiles"].StringValue;
            MailFileExtensions = string.IsNullOrEmpty(zipFileExtensions) ? (new string[] { ".pst" }) : mailFileExtensions.Split(',');

            var otherFileExtensions = _defaultSection["otherfiles"].StringValue;
            OtherFileExtensions = string.IsNullOrEmpty(zipFileExtensions) ? (new string[] { ".ost", ".accdb", ".mdb" }) : otherFileExtensions.Split(',');

            var outputFileName = _defaultSection["outfile"].StringValue;
            OutputFileName = string.IsNullOrEmpty(outputFileName) ? $"PANSearcher_%s.txt" : outputFileName;

            Unmask = _defaultSection["unmask"] != null && _defaultSection["unmask"].BoolValue;

            var excludePans = _defaultSection["excludepans"].StringValue;
            ExcludePans = string.IsNullOrEmpty(zipFileExtensions) ? (new string[] { ".ost", ".accdb", ".mdb" }) : excludePans.Split(',');
        }

        private static string FindSearchBaseByOS() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"C:\" : "/";

        private static string[] FindExcludeFoldersByOS() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? new string[] { @"C:\Windows", @"C:\Program Files", @"C:\Program Files (x86)" } : new string[] { "/mnt" };
    }
}
