using System.Diagnostics;
string sourceFolder = args[0];
string shouldStart = args[1];

CopyFolderContent(sourceFolder, Environment.CurrentDirectory);

static void CopyFolderContent(string folder, string target)
{
    foreach (var item in Directory.GetFileSystemEntries(folder))
    {
        string targetPath = Path.Combine(target, Path.GetRelativePath(folder, item));
        if (Directory.Exists(item))
        {
            Directory.CreateDirectory(targetPath);
            CopyFolderContent(item, targetPath);
        }
        else
        {
            Console.WriteLine(item);
            Console.WriteLine(targetPath);
            //data目录下的不覆盖
            if (item.Contains(Path.PathSeparator + "data" + Path.PathSeparator)
                && !File.Exists(targetPath))
            {
                File.Move(item, targetPath);
            }
            else
            {
                File.Move(item, targetPath, true);
            }
        }
    }
}

Directory.Delete(Directory.GetParent(sourceFolder)!.FullName, true);
if (bool.Parse(shouldStart))
{
    Process.Start("Mikoto.exe");
}
