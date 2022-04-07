namespace PANSearcher.Context
{
    /// <summary>
    ///     The IContext interface abstracts the reading method of the file type by providing only one method to read.
    ///     Each context means a different type of file to read such as plain text files, zip files, etc.
    /// </summary>
    public interface IContext
    {
        IEnumerable<string> Read(string path);
    }
}