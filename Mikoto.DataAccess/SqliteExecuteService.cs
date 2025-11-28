using Microsoft.Data.Sqlite;
using SQLitePCL;

namespace Mikoto.DataAccess
{
    public class SqliteExecuteService : IDisposable
    {
        private readonly SqliteConnection _sqlConnection;
        private readonly string _mDbConnectionString;
        private string? _errorInfo;//最后一次错误信息
        private bool disposedValue;

        static SqliteExecuteService()
        {
            if (Environment.OSVersion.Version.Build >= 10586)
            {
                raw.SetProvider(new SQLite3Provider_winsqlite3());
            }
            else
            {
                raw.SetProvider(new SQLite3Provider_e_sqlite3());
            }
        }


        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="dataSource">数据库文件路径</param>
        public SqliteExecuteService(string dataSource)
        {
            _mDbConnectionString = "Filename=" + dataSource;
            _sqlConnection = new SqliteConnection(_mDbConnectionString);
        }

        /// <summary>
        /// 执行一条非查询语句,失败会返回-1，可通过getLastError获取失败原因
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns>返回影响的结果数</returns>
        public int ExecuteSql(string sql)
        {
            try
            {
                _sqlConnection.Open();
                using var command = new SqliteCommand(sql, _sqlConnection);
                var res = command.ExecuteNonQuery();
                return res;
            }
            catch (SqliteException ex)
            {
                _errorInfo = ex.Message;
                return -1;
            }
        }

        /// <summary>
        /// 执行查询语句，返回单行结果（适用于执行查询可确定只有一条结果的）,失败返回null,可通过getLastError获取失败原因
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="columns">结果应包含的列数</param>
        /// <returns></returns>
        public List<string>? ExecuteReader_OneLine(string sql, int columns)
        {
            try
            {
                _sqlConnection.Open();
                using var cmd = new SqliteCommand(sql, _sqlConnection);
                using var myReader = cmd.ExecuteReader();
                var ret = new List<string>();
                while (myReader.Read())
                {
                    for (var i = 0; i < columns; i++)
                    {
                        ret.Add(myReader[i].ToString() ?? string.Empty);
                    }
                }
                return ret;

            }
            catch (SqliteException e)
            {
                _errorInfo = e.Message;
                return null;
            }
        }

        /// <summary>
        /// 执行查询语句,返回结果,失败返回null,可通过getLastError获取失败原因
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="columns">结果应包含的列数</param>
        /// <returns></returns>
        public List<List<string>>? ExecuteReader(string sql, int columns)
        {
            try
            {
                _sqlConnection.Open();
                using var cmd = new SqliteCommand(sql, _sqlConnection);
                using var myReader = cmd.ExecuteReader();
                var ret = new List<List<string>>();
                while (myReader.Read())
                {
                    var lst = new List<string>();
                    for (var i = 0; i < columns; i++)
                    {
                        lst.Add(myReader[i].ToString() ?? string.Empty);
                    }
                    ret.Add(lst);
                }

                return ret;

            }
            catch (SqliteException e)
            {
                _errorInfo = e.Message;
                return null;
            }

        }

        /// <summary>
        /// 获取最后一次失败原因
        /// </summary>
        /// <returns></returns>
        public string? GetLastError()
        {
            return _errorInfo;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    _sqlConnection.Dispose();
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue=true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~SQLHelper()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
