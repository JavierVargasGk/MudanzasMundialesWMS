using Dapper;
using MudanzasWMS.backend.Common;
using MudanzasWMS.backend.Models;

namespace MudanzasWMS.backend.Repositories;

public class EntradaRepository
{
    private readonly IUnitOfWork _uow;
    public EntradaRepository(IUnitOfWork uow) => _uow = uow;
    public Task<int> CreateAsync(Entrada entrada) =>
        _uow.Connection.ExecuteScalarAsync<int>(
            "INSERT INTO entradas (id_sku, id_cliente, id_ubicacion, id_usuario, cantidad, fecha_ingreso) " +
            "VALUES (@IdSku, @IdCliente, @IdUbicacion, @IdUsuario, @Cantidad, @FechaIngreso) " +
            "RETURNING id_entrada",
            entrada);

    public Task<IEnumerable<Entrada>> ListByClienteAsync(int idCliente, DateTime? desde = null, DateTime? hasta = null) =>
        _uow.Connection.QueryAsync<Entrada>(
            "SELECT id_entrada, id_sku, id_cliente, id_ubicacion, id_usuario, cantidad, " +
            "fecha_ingreso, fecha_registro FROM entradas WHERE id_cliente = @idCliente " +
            "AND (@desde::timestamptz IS NULL OR fecha_registro >= @desde) " +
            "AND (@hasta::timestamptz IS NULL OR fecha_registro <= @hasta) " +
            "ORDER BY fecha_registro DESC",
            new { idCliente, desde, hasta });
}
