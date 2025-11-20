using Npgsql;
using System.Data;

namespace API.Data;

public interface IDbConnectionFactory {
    Task<IDbConnection> CreateConnectionAsync();
}

public class DbConnectionFactory : IDbConnectionFactory {
    private readonly string _connectionString;

    public DbConnectionFactory(string connectionString) {
        _connectionString = connectionString;
    }

    public async Task<IDbConnection> CreateConnectionAsync() {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }
}