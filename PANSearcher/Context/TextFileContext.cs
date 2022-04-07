using System.Text;

namespace PANSearcher.Context
{
    public class TextFileContext : IContext
    {
        public IEnumerable<string>? Read(string path) => File.ReadLines(path, Encoding.UTF8);
    }
}
