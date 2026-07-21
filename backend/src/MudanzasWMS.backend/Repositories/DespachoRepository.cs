using Dapper;
using MudanzasWMS.backend.Common;
using MudanzasWMS.backend.Models;

namespace MudanzasWMS.backend.Repositories;

public class DespachoRepository
{
    private readonly IUnitOfWork _uow;
    public DespachoRepository(IUnitOfWork uow) => _uow = uow;

    private const string BaseSelect =
        "SELECT id_despacho, id_sku, id_cliente, cantidad_solicitada, fecha_salida, " +
        "estado::text AS estado, id_usuario_creacion, id_usuario_confirmacion, " +
        "fecha_creacion, fecha_confirmacion FROM ordenes_despacho ";

    public Task<int> CreatePendienteAsync(int idSku, int idCliente, int cantidadSolicitada, DateTime fechaSalida, int idUsuarioCreacion) =>
        _uow.Connection.ExecuteScalarAsync<int>(
            "INSERT INTO ordenes_despacho (id_sku, id_cliente, cantidad_solicitada, fecha_salida, estado, id_usuario_creacion) " +
            "VALUES (@idSku, @idCliente, @cantidadSolicitada, @fechaSalida, 'pendiente', @idUsuarioCreacion) " +
            "RETURNING id_despacho",
            new { idSku, idCliente, cantidadSolicitada, fechaSalida, idUsuarioCreacion });

    public Task<int> CreateConfirmadoAsync(int idSku, int idCliente, int cantidadSolicitada, DateTime fechaSalida, int idUsuarioCreacion) =>
        _uow.Connection.ExecuteScalarAsync<int>(
            "INSERT INTO ordenes_despacho (id_sku, id_cliente, cantidad_solicitada, fecha_salida, estado, " +
            "id_usuario_creacion, id_usuario_confirmacion, fecha_confirmacion) " +
            "VALUES (@idSku, @idCliente, @cantidadSolicitada, @fechaSalida, 'confirmado', @idUsuarioCreacion, @idUsuarioCreacion, now()) " +
            "RETURNING id_despacho",
            new { idSku, idCliente, cantidadSolicitada, fechaSalida, idUsuarioCreacion });

    public Task<IEnumerable<OrdenDespacho>> ListByClienteAsync(int idCliente) =>
        _uow.Connection.QueryAsync<OrdenDespacho>(
            BaseSelect + "WHERE id_cliente = @idCliente ORDER BY fecha_creacion DESC",
            new { idCliente });

    public Task<OrdenDespacho?> FindByIdAsync(int idDespacho) =>
        _uow.Connection.QuerySingleOrDefaultAsync<OrdenDespacho>(
            BaseSelect + "WHERE id_despacho = @idDespacho", new { idDespacho });
}
