using Microsoft.Extensions.Configuration;
using System;
using System.Data.SqlClient;
using System.IO;

IConfigurationRoot configuration = new ConfigurationBuilder()
     .SetBasePath(Directory.GetCurrentDirectory())
     .AddJsonFile("appsettings.json")
     .Build();

string connectionString = configuration.GetConnectionString("MsSqlConnectionString");

using (SqlConnection connection = new(connectionString))
{
    await connection.OpenAsync();
    Console.WriteLine("Connection is open");
    // Connection info:
    Console.WriteLine("Connection properties:");
    Console.WriteLine($"Connection string: {connection.ConnectionString}");
    Console.WriteLine($"Database: {connection.Database}");
    Console.WriteLine($"Host: {connection.DataSource}");
    Console.WriteLine($"Host version: {connection.ServerVersion}");
    Console.WriteLine($"State: {connection.State}");
    Console.WriteLine($"Workstationld: {connection.WorkstationId}");
}
Console.WriteLine("Connection is closed...");




Console.Read();


//Примеры вставки, обновления, удаления данных.Использование параметров Использование транзакций