using System.Runtime.InteropServices;
using SharpConfig;

namespace PANSearcher
{
    internal class Settings
    {
        public string? SearchBase { get; set; }

        public IEnumerable<string>? ExcludeFolders { get; set; }

        public IEnumerable<string>? TextFileExtensions { get; set; }

        public IEnumerable<string>? ZipFileExtensions { get; set; }

        public IEnumerable<string>? SpecialFileExtensions { get; set; }

        public IEnumerable<string>? MailFileExtensions { get; set; }

        public IEnumerable<string>? OtherFileExtensions { get; set; }

        public string? OutputFileName { get; set; }

        public bool Unmask { get; set; }

        public IEnumerable<string>? ExcludePans { get; set; }

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
        private void Map()
        {
            if (_defaultSection == null) 
            {
                throw new InvalidOperationException("Invalid configuration.");
            }

            var searchBase = _defaultSection["search"].StringValue;
            SearchBase = string.IsNullOrEmpty(searchBase) ? FindSearchBaseByOS() : searchBase;

            var excludeFolders = _defaultSection["exclude"].StringValue;
            if (string.IsNullOrEmpty(excludeFolders))
            {
                excludeFolders = FindExcludeFoldersByOS();
            }
            ExcludeFolders = excludeFolders.Split(',');

            var textFileExtensions = _defaultSection["textfiles"].StringValue;
            if (string.IsNullOrEmpty(textFileExtensions))
            {
                textFileExtensions = ".doc,.xls,.xml,.txt,.csv,.log";
            }
            TextFileExtensions = textFileExtensions.Split(',');

            var zipFileExtensions = _defaultSection["zipfiles"].StringValue;
            if (string.IsNullOrEmpty(zipFileExtensions))
            {
                zipFileExtensions = ".docx,.xlsx,.zip";
            }
            ZipFileExtensions = zipFileExtensions.Split(',');

            var specialFileExtensions = _defaultSection["specialfiles"].StringValue;
            if (string.IsNullOrEmpty(zipFileExtensions))
            {
                specialFileExtensions = ".msg";
            }
            SpecialFileExtensions = specialFileExtensions.Split(',');


            var mailFileExtensions = _defaultSection["mailfiles"].StringValue;
            if (string.IsNullOrEmpty(zipFileExtensions))
            {
                mailFileExtensions = ".pst";
            }
            MailFileExtensions = mailFileExtensions.Split(',');

            var otherFileExtensions = _defaultSection["otherfiles"].StringValue;
            if (string.IsNullOrEmpty(zipFileExtensions))
            {
                otherFileExtensions = ".ost,.accdb,.mdb";
            }
            OtherFileExtensions = otherFileExtensions.Split(',');

            var outputFileName = _defaultSection["outfile"].StringValue;
            if (string.IsNullOrEmpty(outputFileName))
            {
                outputFileName = $"PANSearcher_%s.txt";
            }
            OutputFileName = outputFileName.Replace("%s", DateTime.Now.ToString("yyyy - MM - dd - HHmmss"));

            Unmask = _defaultSection["unmask"] != null && _defaultSection["unmask"].BoolValue;

            var excludePans = _defaultSection["excludepans"].StringValue;
            if (string.IsNullOrEmpty(zipFileExtensions))
            {
                excludePans = ".ost,.accdb,.mdb";
            }
            ExcludePans = excludePans.Split(',');
        }

        private string FindSearchBaseByOS() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"C:\" : "/";

        private string FindExcludeFoldersByOS() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"C:\Windows,C:\Program Files,C:\Program Files (x86)" : "/mnt";
    }
}
