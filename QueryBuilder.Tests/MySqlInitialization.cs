using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

namespace SqlKata.Tests;

public static class MySqlInitialization
{
    public static MySqlTestcontainer DbTestContainer { get; } = GetTestContainer();

    private static MySqlTestcontainer GetTestContainer()
    {
        var database = System.Environment.GetEnvironmentVariable("MYSQL_DATABASE") ?? "master";
        var user = System.Environment.GetEnvironmentVariable("MYSQL_USER") ?? "root";
        var password = System.Environment.GetEnvironmentVariable("MYSQL_PASSWORD") ?? "1";
        var port = System.Environment.GetEnvironmentVariable("MYSQL_Port") ?? "13306";

        var testContainer = new TestcontainersBuilder<MySqlTestcontainer>()
            .WithDatabase(new MySqlTestcontainerConfiguration("mysql/mysql-server:latest")
            {
                Database = database,
                Username = user,
                Password = password,
                Port = int.Parse(port)
            })
            .Build();
        testContainer?.StartAsync().Wait();
        return testContainer;
    }
}
