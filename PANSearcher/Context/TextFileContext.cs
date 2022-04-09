using System.Text;

namespace PANSearcher.Context
{
    public class TextFileContext : IContext
    {
        public IEnumerable<string> Read(string path)
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
