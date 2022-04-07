using System.Runtime.InteropServices;
using System.Text;

namespace PANSearcher
{
    public class Report
    {
        public List<Finding> Findings { get; set; }

        public Report(List<Finding> findings)
        {
            Findings = findings;
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
                .Append("Found ")
                .Append(Findings.Count)
                .AppendLine(" possible PANs.")
                .AppendLine("====================================================================================================")
                .AppendLine()
                .AppendLine(ToString());

            return sb.ToString();
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
