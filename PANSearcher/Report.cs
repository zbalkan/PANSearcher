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

        public string Prepare(string searchBase, string excluded, string command)
        {
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
