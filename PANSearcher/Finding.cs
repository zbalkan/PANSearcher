using System.Text;

namespace PANSearcher
{
    public class Finding
    {
        public string FilePath { get; set; }

        public List<Record> Records { get; set; }

        private FileInfo? fileInfo;

        private FileInfo GetFileInfo()
        {
            if (fileInfo == null)
            {
                fileInfo = new FileInfo(FilePath);
            }

            return fileInfo;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            _ = sb.Append("FOUND PANs: ")
                .Append(GetFileInfo().FullName)
                .Append(" (")
                .Append(GetFileInfo().Length / 1024)
                .Append("KB ")
                .Append(GetFileInfo().CreationTime)
                .AppendLine(")");

            foreach (var record in Records)
            {
                _ = sb.Append('\t').AppendLine(record.ToString());
            }

            return sb.ToString();
        }
    }
}
