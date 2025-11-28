namespace Mikoto.DataAccess;

public class FileService : IFileService
{
    public void WriteAllLines(string path, IEnumerable<string> lines)
        => File.WriteAllLines(path, lines);
}
