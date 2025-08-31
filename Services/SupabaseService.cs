using Npgsql;
using Dapper;
using System.Data;

public class SupabaseService
{
    private readonly string _connectionString;

    public SupabaseService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);
}