using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;

IConfigurationRoot configuration = new ConfigurationBuilder()
     .SetBasePath(Directory.GetCurrentDirectory())
     .AddJsonFile("appsettings.json")
     .Build();

string connectionString = configuration.GetConnectionString("MsSqlConnectionString");


#region Connection Info
using (SqlConnection connection = new(connectionString))
{
    await connection.OpenAsync();
    Console.WriteLine("Connection is open");
    Console.WriteLine("Connection properties:");
    Console.WriteLine($"Connection string: {connection.ConnectionString}");
    Console.WriteLine($"Database: {connection.Database}");
    Console.WriteLine($"Host: {connection.DataSource}");
    Console.WriteLine($"Host version: {connection.ServerVersion}");
    Console.WriteLine($"State: {connection.State}");
    Console.WriteLine($"Workstationld: {connection.WorkstationId}");
}
#endregion


#region Create / Update / Delete Data
using (SqlConnection connection = new(connectionString))
{
    await connection.OpenAsync();
    SqlCommand command = new();
    command.CommandText = "CREATE DATABASE library";
    command.Connection = connection;
    await command.ExecuteNonQueryAsync();
}

using (SqlConnection connection = new(connectionString))
{
    await connection.OpenAsync();
    SqlCommand command = new();
    command.CommandText = @"CREATE TABLE Books (
Id INT PRIMARY KEY IDENTITY,
Title NVARCHAR(100) NOT NULL,
Author NVARCHAR(100) NOT NULL,
PageCount INT NOT NULL)";
    command.Connection = connection;
    await command.ExecuteNonQueryAsync();
}

using (SqlConnection connection = new(connectionString))
{
    await connection.OpenAsync();
    SqlCommand command = new(
        @"INSERT INTO Books (Title, Author, PageCount) VALUES
                            ('To Kill a Mockingbird', 'Harper Lee', 150),
                            ('1984', 'George Orwell', 98),
                            ('The Great Gatsby', 'F. Scott Fitzgerald', 321)", connection);
    int number = await command.ExecuteNonQueryAsync();
    Console.WriteLine($"Added objects count: {number}");
}

using (SqlConnection connection = new(connectionString))
{
    await connection.OpenAsync();
    SqlCommand command = new(@"UPDATE Books SET PageCount=120 WHERE Title='1984'", connection);
    int number = await command.ExecuteNonQueryAsync();
    Console.WriteLine($"Updated objects count: {number}");
}

using (SqlConnection connection = new(connectionString))
{
    await connection.OpenAsync();
    SqlCommand command = new(@"DELETE FROM Books WHERE Title='1984'", connection);
    int number = await command.ExecuteNonQueryAsync();
    Console.WriteLine($"Deleted objects count: {number}");
}
#endregion


#region Read Data
using (SqlConnection connection = new(connectionString))
{
    await connection.OpenAsync();

    SqlCommand command = new("SELECT * FROM Books", connection);
    using (SqlDataReader reader = await command.ExecuteReaderAsync())
    {
        if (reader.HasRows)
        {
            string columnName1 = reader.GetName(0);
            string columnName2 = reader.GetName(1);
            string columnName3 = reader.GetName(2);
            string columnName4 = reader.GetName(3);

            Console.WriteLine($"{columnName1}\t{columnName3}\t{columnName2}\t{columnName3}\t{columnName4}");

            while (await reader.ReadAsync())
            {
                int id = reader.GetInt32(0);
                string title = reader.GetString(2);
                string author = reader.GetString(1);
                int pageCount = reader.GetInt32(3);

                Console.WriteLine($"{id} \t{title} \t{author} \t{pageCount}");
            }
        }
    }
}

using (SqlConnection connection = new(connectionString))
{
    await connection.OpenAsync();

    SqlCommand command = new("SELECT COUNT(*) FROM Books", connection);
    object count = await command.ExecuteScalarAsync();

    command.CommandText = "SELECT MIN(PageCount) FROM Books";
    object minBook = await command.ExecuteScalarAsync();

    Console.WriteLine($"Sum objects count: {count}");
    Console.WriteLine($"Min Page Count: {minBook}");
}
#endregion


#region Parameterized Queries
string sqlExpression = "INSERT INTO Books (Title, Author, PageCount) VALUES (@title, @author, @pageCount);SET @id=SCOPE_IDENTITY()";
using (SqlConnection connection = new(connectionString))
{
    await connection.OpenAsync();

    SqlCommand command = new(sqlExpression, connection);

    SqlParameter titleParam = new("@title", "Jane Eyre");
    command.Parameters.Add(titleParam);

    SqlParameter authorParam = new("@author", "Charlotte Brontë");
    command.Parameters.Add(authorParam);

    SqlParameter pageCountParam = new("@pageCount", 215);
    command.Parameters.Add(pageCountParam);

    SqlParameter idParam = new()
    {
        ParameterName = "@id",
        SqlDbType = SqlDbType.Int,
        Direction = ParameterDirection.Output
    };
    command.Parameters.Add(idParam);

    int number = await command.ExecuteNonQueryAsync();
    Console.WriteLine($"Added objects count: {number}");
    Console.WriteLine($"New book Id: {idParam.Value}");
}
#endregion


#region Transactions
using (SqlConnection connection = new(connectionString))
{
    await connection.OpenAsync();
    SqlTransaction transaction = connection.BeginTransaction();

    SqlCommand command = connection.CreateCommand();
    command.Transaction = transaction;

    try
    {
        command.CommandText = "INSERT INTO Books (Title, Author, PageCount) VALUES ('Book1', 'Author1', 5)";
        await command.ExecuteNonQueryAsync();
        command.CommandText = "INSERT INTO Books (Title, Author, PageCount) VALUES ('Book2', 'Author2', 55)";
        await command.ExecuteNonQueryAsync();

        await transaction.CommitAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        await transaction.RollbackAsync();
    }
}
#endregion

Console.Read();