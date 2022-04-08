using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace PANSearcher
{
    public class Report
    {
        public List<Finding> Findings { get; set; }

        public int NumberOfFiles { get; set; }

        public Report(List<Finding> findings, int numberOfFiles = 0)
        {
            Findings = findings;
            NumberOfFiles = numberOfFiles;
        }

        public Report()
        {
            Findings = new List<Finding>();
            NumberOfFiles = 0;
        }

        public string Prepare()
        {
            var searchBase = Settings.Instance.SearchBase;
#pragma warning disable CS8604 // Possible null reference argument.
            var excluded = string.Join(' ', Settings.Instance.ExcludeFolders);
#pragma warning restore CS8604 // Possible null reference argument.
            var args = new string[Environment.GetCommandLineArgs().Length - 1];
            for (var i = 0; i < args.Length; i++)
            {
                args[i] = Environment.GetCommandLineArgs()[i + 1];
            }

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var command = $"{Process.GetCurrentProcess().MainModule.ModuleName} {string.Join(' ', args)}";
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            var sb = new StringBuilder();

            sb.Append("PANSearcher Report - ")
                .AppendLine(DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy"))
                .AppendLine("====================================================================================================")
                .Append("Searched: ")
                .AppendLine(searchBase)
                .Append("Excluded: ")
                .AppendLine(excluded)
                .Append("Command: ")
                .AppendLine(command)
                .Append("Uname: ")
                .Append(Environment.OSVersion.Platform)
                .Append(" | ")
                .Append(Environment.MachineName)
                .Append(" | ")
                .Append(Environment.OSVersion.Version)
                .Append(" | ")
                .Append(RuntimeInformation.ProcessArchitecture)
                .Append(" | ")
                .AppendLine(Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER"))
                .Append("Searched ")
                .Append(NumberOfFiles)
                .Append("files. Found ")
                .Append(Findings.Count)
                .AppendLine(" possible PANs.")
                .AppendLine("====================================================================================================")
                .AppendLine()
                .AppendLine(ToString());

            return sb.ToString();
        }

        public void ImportFrom(Report report)
        {
            Findings.AddRange(report.Findings);

            NumberOfFiles += report.NumberOfFiles;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var finding in Findings)
            {
                sb.AppendLine(finding.ToString());
            }

            return sb.ToString();
        }
    }
}
