using Dapper;
using MudanzasWMS.backend.Common;
using MudanzasWMS.backend.Models;

namespace MudanzasWMS.backend.Repositories;


public class AuditoriaRepository
{
    private readonly IUnitOfWork _uow;
    public AuditoriaRepository(IUnitOfWork uow) => _uow = uow;

    public Task<IEnumerable<Auditoria>> ListAsync(string? tablaAfectada = null, int take = 200) =>
        _uow.Connection.QueryAsync<Auditoria>(
            "SELECT id_auditoria, id_usuario, tabla_afectada, id_entidad_afectada, accion, " +
            "valores_anteriores::text AS valores_anteriores, valores_nuevos::text AS valores_nuevos, fecha " +
            "FROM auditoria WHERE (@tablaAfectada::text IS NULL OR tabla_afectada = @tablaAfectada) " +
            "ORDER BY fecha DESC LIMIT @take",
            new { tablaAfectada, take });
}
