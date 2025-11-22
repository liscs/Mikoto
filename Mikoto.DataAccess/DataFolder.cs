namespace Mikoto.DataAccess
{
    public class DataFolder
    {
        /// <summary>
        /// 获取数据存储目录，自动迁移旧目录数据到标准化位置
        /// </summary>
        public static string Path
        {
            get
            {
                // 新的标准化目录
                string newFolder = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Mikoto",
                    "data"
                );
                Directory.CreateDirectory(newFolder);

                // 旧的程序根目录 data
                string oldFolder = System.IO.Path.GetFullPath("data");

                // 只迁移一次
                string migrationMarker = System.IO.Path.Combine(newFolder, ".migrated");
                if (!File.Exists(migrationMarker) && Directory.Exists(oldFolder))
                {
                    MigrateData(oldFolder, newFolder);
                    File.WriteAllText(migrationMarker, "migrated");
                }

                return newFolder;
            }
        }

        /// <summary>
        /// 将旧目录数据迁移到新目录
        /// </summary>
        private static void MigrateData(string oldFolder, string newFolder)
        {
            foreach (var dirPath in Directory.GetDirectories(oldFolder, "*", SearchOption.AllDirectories))
            {
                string newDir = dirPath.Replace(oldFolder, newFolder);
                Directory.CreateDirectory(newDir);
            }

            foreach (var filePath in Directory.GetFiles(oldFolder, "*.*", SearchOption.AllDirectories))
            {
                string newFile = filePath.Replace(oldFolder, newFolder);
                File.Copy(filePath, newFile, overwrite: true);
            }
        }
    }
}
