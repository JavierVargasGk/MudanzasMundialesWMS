using Dapper;
using MudanzasWMS.backend.Common;
using MudanzasWMS.backend.Models;

namespace MudanzasWMS.backend.Repositories;

public class DiscrepanciaRepository
{
    private readonly IUnitOfWork _uow;
    public DiscrepanciaRepository(IUnitOfWork uow) => _uow = uow;

    private const string BaseSelect =
        "SELECT id_reporte, id_sku, id_cliente, id_ubicacion, id_usuario_reporta, " +
        "tipo_discrepancia::text AS tipo_discrepancia, observacion, estado::text AS estado, " +
        "id_ajuste_resultante, fecha_reporte, fecha_resolucion FROM reportes_discrepancia ";

    public Task<int> CreateAsync(int idSku, int idCliente, int idUbicacion, int idUsuarioReporta,
        Models.Enums.TipoDiscrepancia tipo, string observacion) =>
        _uow.Connection.ExecuteScalarAsync<int>(
            "INSERT INTO reportes_discrepancia (id_sku, id_cliente, id_ubicacion, id_usuario_reporta, " +
            "tipo_discrepancia, observacion) " +
            "VALUES (@idSku, @idCliente, @idUbicacion, @idUsuarioReporta, @tipo::tipo_discrepancia, @observacion) " +
            "RETURNING id_reporte",
            new { idSku, idCliente, idUbicacion, idUsuarioReporta, tipo = tipo.ToString(), observacion });

    public Task<IEnumerable<ReporteDiscrepancia>> ListByClienteAsync(int idCliente, bool soloAbiertos = false) =>
        _uow.Connection.QueryAsync<ReporteDiscrepancia>(
            BaseSelect + "WHERE id_cliente = @idCliente " +
            (soloAbiertos ? "AND estado = 'abierto' " : "") + "ORDER BY fecha_reporte DESC",
            new { idCliente });

    public Task<ReporteDiscrepancia?> FindByIdAsync(int idReporte) =>
        _uow.Connection.QuerySingleOrDefaultAsync<ReporteDiscrepancia>(
            BaseSelect + "WHERE id_reporte = @idReporte", new { idReporte });

    public Task ResolverAsync(int idReporte, int? idAjusteResultante) =>
        _uow.Connection.ExecuteAsync(
            "UPDATE reportes_discrepancia SET estado = 'resuelto', id_ajuste_resultante = @idAjusteResultante, " +
            "fecha_resolucion = now() WHERE id_reporte = @idReporte",
            new { idReporte, idAjusteResultante });

    public Task DescartarAsync(int idReporte) =>
        _uow.Connection.ExecuteAsync(
            "UPDATE reportes_discrepancia SET estado = 'descartado', fecha_resolucion = now() " +
            "WHERE id_reporte = @idReporte",
            new { idReporte });
}
