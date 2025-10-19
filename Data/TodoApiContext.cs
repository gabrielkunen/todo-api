using System.Data;
using Npgsql;

namespace TodoApi.Data;

public class TodoApiContext
{
    private readonly IConfiguration _configuration;
    private readonly string _connection;
    
    public TodoApiContext(IConfiguration configuration)
    {
        _configuration = configuration;
        _connection = configuration.GetConnectionString("DefaultConnection")!;
    }
    
    public IDbConnection CreateConnection() => new NpgsqlConnection(_connection);
}