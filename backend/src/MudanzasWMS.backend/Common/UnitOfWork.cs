using Dapper;
using MudanzasWMS.backend.Data;
using Npgsql;
using System.Data;

namespace MudanzasWMS.backend.Common;

public class UnitOfWork : IUnitOfWork
{
    private readonly IDbConnectionFactory _factory;
    private NpgsqlConnection? _connection;
    private NpgsqlTransaction? _transaction;
    private bool _contextSet;
    private bool _finished;

    public UnitOfWork(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public IDbConnection Connection =>
        _connection ?? throw new InvalidOperationException(
            "Call EnsureOpenAsync or EnsureLoginStageAsync before using Connection.");

    private async Task EnsureTransactionAsync()
    {
        if (_connection is null)
        {
            _connection = await _factory.CreateOpenConnectionAsync();
            _transaction = await _connection.BeginTransactionAsync();
        }
    }

    public async Task EnsureOpenAsync(RequestContext ctx, int? clienteId = null)
    {
        await EnsureTransactionAsync();

        var effectiveClienteId = clienteId ?? ctx.ClienteId;
        if (!_contextSet || clienteId is not null)
        {
            await Connection.ExecuteAsync(new CommandDefinition(
                "SELECT set_config('app.current_user_role', @role, true), " +
                "set_config('app.current_user_id', @userId, true), " +
                "set_config('app.current_cliente_id', @clienteId, true);",
                new
                {
                    role = ctx.Role,
                    userId = ctx.UserId.ToString(),
                    clienteId = effectiveClienteId?.ToString() ?? ""
                },
                _transaction));
            _contextSet = true;
        }
    }

    public async Task EnsureLoginStageAsync()
    {
        await EnsureTransactionAsync();
        await Connection.ExecuteAsync(new CommandDefinition(
            "SELECT set_config('app.auth_stage', 'login', true);",
            transaction: _transaction));
    }

    public async Task CommitAsync()
    {
        if (_transaction is not null && !_finished)
        {
            await _transaction.CommitAsync();
            _finished = true;
        }
    }

    public async Task RollbackAsync()
    {
        if (_transaction is not null && !_finished)
        {
            await _transaction.RollbackAsync();
            _finished = true;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction is not null) await _transaction.DisposeAsync();
        if (_connection is not null) await _connection.DisposeAsync();
    }
}
