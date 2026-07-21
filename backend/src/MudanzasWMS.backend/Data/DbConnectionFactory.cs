using Npgsql;

namespace MudanzasWMS.backend.Data;

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly NpgsqlDataSource _dataSource;

    public DbConnectionFactory(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<NpgsqlConnection> CreateOpenConnectionAsync()
        => await _dataSource.OpenConnectionAsync();
}
