namespace Mikoto.DataAccess;

public interface IFileService
{
    void WriteAllLines(string path, IEnumerable<string> lines);
}
