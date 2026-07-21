using System.Data;

namespace MudanzasWMS.backend.Common;

public interface IUnitOfWork : IAsyncDisposable
{
    IDbConnection Connection { get; }
    Task EnsureOpenAsync(RequestContext ctx, int? clienteId = null);
    Task EnsureLoginStageAsync();

    Task CommitAsync();
    Task RollbackAsync();
}
