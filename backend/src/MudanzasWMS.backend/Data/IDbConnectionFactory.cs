using Npgsql;

namespace MudanzasWMS.backend.Data;

public interface IDbConnectionFactory
{
    Task<NpgsqlConnection> CreateOpenConnectionAsync();
}
