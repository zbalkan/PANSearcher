using System.Text;

namespace PANSearcher
{
    public class Finding
    {
        public string FilePath { get; set; }

        public List<Record> Records { get; set; }

        private FileInfo? fileInfo;

        private FileInfo FileInfo
        {
            get
            {
                if (fileInfo == null)
                {
                    fileInfo = new FileInfo(FilePath);
                }

                return fileInfo;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            _ = sb.Append("FOUND PANs: ")
                .Append(FileInfo.FullName)
                .Append(" (")
                .Append(FileInfo.Length / 1024)
                .Append("KB ")
                .Append(FileInfo.CreationTime)
                .AppendLine(")");

            for (var i = 0; i < Records.Count; i++)
            {
                var record = Records[i];
                _ = sb.Append('\t').AppendLine(record.ToString());
            }

            return sb.ToString();
        }
    }
}
