using System.Runtime.InteropServices;
using SharpConfig;

namespace PANSearcher
{
    internal class Config
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

        private readonly Section _defaultConfig;

        public Config(string path)
        {
            var config = Configuration.LoadFromFile(path);
            _defaultConfig = config["Default"];
            Map();
        }

        public Config()
        {
            var config = new Configuration
            {
                new Section("Default")
            };
            _defaultConfig = config["Default"];
            Map();
        }

        private void Map()
        {
            var searchBase = _defaultConfig["search"].StringValue;
            SearchBase = string.IsNullOrEmpty(searchBase) ? FindSearchBaseByOS() : searchBase;

            var excludeFolders = _defaultConfig["exclude"].StringValue;
            if (string.IsNullOrEmpty(excludeFolders))
            {
                excludeFolders = FindExcludeFoldersByOS();
            }
            ExcludeFolders = excludeFolders.Split(',');

            var textFileExtensions = _defaultConfig["textfiles"].StringValue;
            if (string.IsNullOrEmpty(textFileExtensions))
            {
                textFileExtensions = ".doc,.xls,.xml,.txt,.csv,.log";
            }
            TextFileExtensions = textFileExtensions.Split(',');

            var zipFileExtensions = _defaultConfig["zipfiles"].StringValue;
            if (string.IsNullOrEmpty(zipFileExtensions))
            {
                zipFileExtensions = ".docx,.xlsx,.zip";
            }
            ZipFileExtensions = zipFileExtensions.Split(',');

            var specialFileExtensions = _defaultConfig["specialfiles"].StringValue;
            if (string.IsNullOrEmpty(zipFileExtensions))
            {
                specialFileExtensions = ".msg";
            }
            SpecialFileExtensions = specialFileExtensions.Split(',');


            var mailFileExtensions = _defaultConfig["mailfiles"].StringValue;
            if (string.IsNullOrEmpty(zipFileExtensions))
            {
                mailFileExtensions = ".pst";
            }
            MailFileExtensions = mailFileExtensions.Split(',');

            var otherFileExtensions = _defaultConfig["otherfiles"].StringValue;
            if (string.IsNullOrEmpty(zipFileExtensions))
            {
                otherFileExtensions = ".ost,.accdb,.mdb";
            }
            OtherFileExtensions = otherFileExtensions.Split(',');

            var outputFileName = _defaultConfig["outfile"].StringValue;
            if (string.IsNullOrEmpty(outputFileName))
            {
                outputFileName = $"PANSearcher_%s.txt";
            }
            OutputFileName = outputFileName.Replace("%s", DateTime.Now.ToString("yyyy - MM - dd - HHmmss"));

            Unmask = _defaultConfig["unmask"] != null && _defaultConfig["unmask"].BoolValue;

            var excludePans = _defaultConfig["excludepans"].StringValue;
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
