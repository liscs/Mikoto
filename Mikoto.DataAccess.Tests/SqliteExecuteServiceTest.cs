using System;
using System.Collections.Generic;
using System.Linq; 
using Xunit;

namespace Mikoto.DataAccess.Tests;

// 为了在每个测试方法运行后清理资源 (如内存数据库连接)，我们实现 IDisposable
public class SqliteExecuteServiceTest : IDisposable
{
    private readonly SqliteExecuteService sqlService;
    private readonly string connectionString;

    public SqliteExecuteServiceTest()
    {
        // 使用一个唯一的内存数据库连接字符串
        connectionString = $"Data Source=file:memdb1{Guid.NewGuid()}?mode=memory&cache=shared";
        sqlService = new SqliteExecuteService(connectionString);

        // 创建测试表
        string createTableSql = @"
            CREATE TABLE test_table(
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT,
                age INTEGER
            );";
        sqlService.ExecuteSql(createTableSql);
    }

    // 实现 IDisposable 替代 [TestCleanup]
    public void Dispose()
    {
        // 可以在这里执行清理工作，例如关闭连接或清理文件数据库（如果不是内存数据库）
        // 对于内存模式的 SQLite，关闭连接通常会销毁数据库。
        sqlService.Dispose();
    }

    [Fact]
    public void ExecuteSql_ShouldInsertRow()
    {
        int res = sqlService.ExecuteSql("INSERT INTO test_table (name, age) VALUES ('Alice', 20);");
        Assert.Equal(1, res); 

        var rows = sqlService.ExecuteReader("SELECT name, age FROM test_table WHERE name='Alice';", 2);

        Assert.NotNull(rows); 
        Assert.Single(rows);
        Assert.Equal("Alice", rows[0][0]);
        Assert.Equal("20", rows[0][1]);
    }

    [Fact]
    public void ExecuteReader_OneLine_ShouldReturnSingleRow()
    {
        sqlService.ExecuteSql("INSERT INTO test_table (name, age) VALUES ('Bob', 25);");

        var row = sqlService.ExecuteReader_OneLine("SELECT name, age FROM test_table WHERE name='Bob';", 2);

        Assert.NotNull(row);
        // 对于 HasCount(2, row)，我们使用 Assert.Equal(2, row.Count)
        Assert.Equal(2, row.Count);
        Assert.Equal("Bob", row[0]);
        Assert.Equal("25", row[1]);
    }

    [Fact]
    public void ExecuteReader_ShouldReturnMultipleRows()
    {
        sqlService.ExecuteSql("INSERT INTO test_table (name, age) VALUES ('Charlie', 30);");
        sqlService.ExecuteSql("INSERT INTO test_table (name, age) VALUES ('David', 35);");

        var rows = sqlService.ExecuteReader("SELECT name, age FROM test_table;", 2);

        Assert.NotNull(rows);
        Assert.Equal(2, rows.Count);
        Assert.Equal("Charlie", rows[0][0]);
        Assert.Equal("30", rows[0][1]);
        Assert.Equal("David", rows[1][0]);
        Assert.Equal("35", rows[1][1]);
    }

    [Fact]
    public void ExecuteSql_ShouldReturnMinusOne_OnInvalidSql()
    {
        int res = sqlService.ExecuteSql("INSERT INTO non_exist_table (a) VALUES (1);");

        Assert.Equal(-1, res);
        Assert.NotNull(sqlService.GetLastError());
        Console.WriteLine(sqlService.GetLastError());
    }
}