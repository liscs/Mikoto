namespace Mikoto.DataAccess.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

[TestClass]
[DoNotParallelize]
public class SQLHelperTest
{
    private SqliteExecuteService sqlService = default!;

    [TestInitialize]
    public void Setup()
    {
        sqlService = new SqliteExecuteService($"Data Source=file:memdb1{Guid.NewGuid()}?mode=memory");

        // 创建测试表
        string createTableSql = @"
            CREATE TABLE test_table(
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT,
                age INTEGER
            );";
        sqlService.ExecuteSql(createTableSql);
    }

    [TestMethod]
    public void ExecuteSql_ShouldInsertRow()
    {
        int res = sqlService.ExecuteSql("INSERT INTO test_table (name, age) VALUES ('Alice', 20);");
        Assert.AreEqual(1, res);

        var rows = sqlService.ExecuteReader("SELECT name, age FROM test_table WHERE name='Alice';", 2);
        Assert.IsNotNull(rows);
        Assert.HasCount(1, rows);
        Assert.AreEqual("Alice", rows[0][0]);
        Assert.AreEqual("20", rows[0][1]);
    }

    [TestMethod]
    public void ExecuteReader_OneLine_ShouldReturnSingleRow()
    {
        sqlService.ExecuteSql("INSERT INTO test_table (name, age) VALUES ('Bob', 25);");

        var row = sqlService.ExecuteReader_OneLine("SELECT name, age FROM test_table WHERE name='Bob';", 2);
        Assert.IsNotNull(row);
        Assert.HasCount(2, row);
        Assert.AreEqual("Bob", row[0]);
        Assert.AreEqual("25", row[1]);
    }

    [TestMethod]
    public void ExecuteReader_ShouldReturnMultipleRows()
    {
        sqlService.ExecuteSql("INSERT INTO test_table (name, age) VALUES ('Charlie', 30);");
        sqlService.ExecuteSql("INSERT INTO test_table (name, age) VALUES ('David', 35);");

        var rows = sqlService.ExecuteReader("SELECT name, age FROM test_table;", 2);
        Assert.IsNotNull(rows);
        Assert.HasCount(2, rows);
        Assert.AreEqual("Charlie", rows[0][0]);
        Assert.AreEqual("30", rows[0][1]);
        Assert.AreEqual("David", rows[1][0]);
        Assert.AreEqual("35", rows[1][1]);
    }

    [TestMethod]
    public void ExecuteSql_ShouldReturnMinusOne_OnInvalidSql()
    {
        int res = sqlService.ExecuteSql("INSERT INTO non_exist_table (a) VALUES (1);");
        Assert.AreEqual(-1, res);
        Assert.IsNotNull(sqlService.GetLastError());
        Console.WriteLine(sqlService.GetLastError());
    }
}
