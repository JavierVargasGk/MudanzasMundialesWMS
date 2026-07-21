using Dapper;
using MudanzasWMS.backend.Common;
using MudanzasWMS.backend.Models;

namespace MudanzasWMS.backend.Repositories;

public class AjusteRepository
{
    private readonly IUnitOfWork _uow;
    public AjusteRepository(IUnitOfWork uow) => _uow = uow;

    public Task<int> CreateAsync(int idSku, int idCliente, int idUbicacion, int idUsuarioSolicitante,
        Models.Enums.TipoAjuste tipo, int cantidadAjuste, string razon) =>
        _uow.Connection.ExecuteScalarAsync<int>(
            "INSERT INTO ajustes_inventario (id_sku, id_cliente, id_ubicacion, id_usuario_solicitante, " +
            "tipo_ajuste, cantidad_ajuste, razon) " +
            "VALUES (@idSku, @idCliente, @idUbicacion, @idUsuarioSolicitante, @tipo::tipo_ajuste, @cantidadAjuste, @razon) " +
            "RETURNING id_ajuste",
            new { idSku, idCliente, idUbicacion, idUsuarioSolicitante, tipo = tipo.ToString(), cantidadAjuste, razon });

    public Task<IEnumerable<AjusteInventario>> ListByClienteAsync(int idCliente) =>
        _uow.Connection.QueryAsync<AjusteInventario>(
            "SELECT id_ajuste, id_sku, id_cliente, id_ubicacion, id_usuario_solicitante, " +
            "tipo_ajuste::text AS tipo_ajuste, cantidad_ajuste, razon, fecha_registro " +
            "FROM ajustes_inventario WHERE id_cliente = @idCliente ORDER BY fecha_registro DESC",
            new { idCliente });
}
