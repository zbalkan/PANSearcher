using System.IO.Compression;
using System.Text;

namespace PANSearcher.Context
{
    public class ZipFileContext : IContext
    {
        public IEnumerable<string> Read(string path)
        {
            var extractPath = string.Empty;

            if (IsOfficeFile(path))
            {
                extractPath = Extract(path);

                string? document = null;
                var ext = Path.GetExtension(path);

                switch (ext)
                {
                    case ".docx":
                        document = Path.Combine(extractPath, "word", "document.xml");
                        return ReadText(document);
                    case ".xlsx":
                        var worksheets = Directory.GetFiles(Path.Combine(extractPath, "xl","worksheets"), "*.xml");
                        var lines = new List<string>();
                        foreach (var ws in worksheets)
                        {
                            lines.AddRange(ReadText(ws));
                        }
                        return lines;
                }

                if (document != null)
                {
                    return ReadText(document);
                }
            }
            else
            {
                Print.Verbose("Not implemented");
            }

            if (!string.IsNullOrEmpty(extractPath))
            {
                Cleanup(extractPath);
            }

            return Array.Empty<string>();
        }

        private bool IsOfficeFile(string path)
        {
            var ext = Path.GetExtension(path);
            return ext == ".docx" || ext == ".xlsx";
        }

        private string Extract(string path)
        {
            // Generate the path.
            var guid = Guid.NewGuid();
            var tempFolderPath = Path.GetTempPath();

            var extractPath = Directory.CreateDirectory(Path.Combine(tempFolderPath, guid.ToString())).FullName;

            using var archive = ZipFile.OpenRead(path);

            try
            {
                ZipFile.ExtractToDirectory(path, extractPath);
            }
            catch
            {
                // Ignore
            }

            return extractPath;
        }

        private void Cleanup(string extractPath) => Directory.Delete(extractPath, true);

        private IEnumerable<string> ReadText(string path)
        {
            IEnumerable<string> lines = new string[] { string.Empty };
            try
            {
                lines = File.ReadLines(path, Encoding.UTF8);
            }
            catch (Exception e)
            {
                Print.Verbose(e.Message);
            }
            return lines;
        }
    }
}
