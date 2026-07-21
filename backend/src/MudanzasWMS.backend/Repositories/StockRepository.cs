using Dapper;
using MudanzasWMS.backend.Common;
using MudanzasWMS.backend.Models;
using MudanzasWMS.backend.Models.Enums;

namespace MudanzasWMS.backend.Repositories;

public class StockRepository
{
    private readonly IUnitOfWork _uow;
    public StockRepository(IUnitOfWork uow) => _uow = uow;

    public Task<IEnumerable<StockActual>> ListByClienteAsync(int idCliente) =>
        _uow.Connection.QueryAsync<StockActual>(
            "SELECT s.id_stock AS IdStock, s.id_sku AS IdSku, s.id_cliente AS IdCliente, " +
            "s.id_ubicacion AS IdUbicacion, s.cantidad_actual AS CantidadActual, " +
            "s.fecha_actualizacion AS FechaActualizacion, s.numero_lote AS NumeroLote, " +
            "s.status::text AS Status, k.codigo_sku AS CodigoSku, k.nombre AS NombreSku, " +
            "u.codigo AS CodigoUbicacion " +
            "FROM stock_actual s " +
            "JOIN skus k ON k.id_sku = s.id_sku " +
            "JOIN ubicaciones u ON u.id_ubicacion = s.id_ubicacion " +
            "WHERE s.id_cliente = @idCliente ORDER BY k.codigo_sku, u.codigo",
            new { idCliente });

    public async Task<int> GetCantidadDisponibleForUpdateAsync(int idSku, int idCliente)
    {
        var cantidades = await _uow.Connection.QueryAsync<int>(
            "SELECT cantidad_actual FROM stock_actual WHERE id_sku = @idSku AND id_cliente = @idCliente FOR UPDATE",
            new { idSku, idCliente });
        return cantidades.Sum();
    }

    public Task IncrementarAsync(int idSku, int idCliente, int idUbicacion, int cantidad, string? numeroLote = null, EstadoLote status = EstadoLote.Disponible) =>
        _uow.Connection.ExecuteAsync(
            "INSERT INTO stock_actual (id_sku, id_cliente, id_ubicacion, cantidad_actual, numero_lote, status) " +
            "VALUES (@idSku, @idCliente, @idUbicacion, @cantidad, @numeroLote, @status) " +
            "ON CONFLICT (id_sku, id_ubicacion) DO UPDATE SET " +
            "cantidad_actual = stock_actual.cantidad_actual + EXCLUDED.cantidad_actual, " +
            "numero_lote = COALESCE(EXCLUDED.numero_lote, stock_actual.numero_lote), " +
            "status = EXCLUDED.status, " +
            "fecha_actualizacion = now()",
            new { idSku, idCliente, idUbicacion, cantidad, numeroLote, status = status.ToString() });

    public async Task DecrementarAsync(int idSku, int idCliente, int cantidadADescontar)
    {
        var filas = (await _uow.Connection.QueryAsync<StockUbicacionCantidad>(
            "SELECT id_ubicacion AS IdUbicacion, cantidad_actual AS Cantidad FROM stock_actual " +
            "WHERE id_sku = @idSku AND id_cliente = @idCliente AND cantidad_actual > 0 " +
            "ORDER BY fecha_actualizacion ASC",
            new { idSku, idCliente })).ToList();

        var restante = cantidadADescontar;
        foreach (var fila in filas)
        {
            if (restante <= 0) break;
            var descuento = Math.Min(restante, fila.Cantidad);
            await _uow.Connection.ExecuteAsync(
                "UPDATE stock_actual SET cantidad_actual = cantidad_actual - @descuento, fecha_actualizacion = now() " +
                "WHERE id_sku = @idSku AND id_cliente = @idCliente AND id_ubicacion = @idUbicacion",
                new { descuento, idSku, idCliente, idUbicacion = fila.IdUbicacion });
            restante -= descuento;
        }

        if (restante > 0)
        {
            throw new InvalidOperationException(
                "Stock insuficiente al momento de descontar. El llamador debe validar " +
                "GetCantidadDisponibleForUpdateAsync antes de invocar este metodo.");
        }
    }

    public Task AplicarAjusteAsync(int idSku, int idCliente, int idUbicacion, int cantidadAjuste, string? numeroLote = null, EstadoLote status = EstadoLote.Disponible) =>
        _uow.Connection.ExecuteAsync(
            "INSERT INTO stock_actual (id_sku, id_cliente, id_ubicacion, cantidad_actual, numero_lote, status) " +
            "VALUES (@idSku, @idCliente, @idUbicacion, GREATEST(@cantidadAjuste, 0), @numeroLote, @status) " +
            "ON CONFLICT (id_sku, id_ubicacion) DO UPDATE SET " +
            "cantidad_actual = GREATEST(stock_actual.cantidad_actual + @cantidadAjuste, 0), " +
            "numero_lote = COALESCE(EXCLUDED.numero_lote, stock_actual.numero_lote), " +
            "status = EXCLUDED.status, " +
            "fecha_actualizacion = now()",
            new { idSku, idCliente, idUbicacion, cantidadAjuste, numeroLote, status = status.ToString() });

    private class StockUbicacionCantidad
    {
        public int IdUbicacion { get; set; }
        public int Cantidad { get; set; }
    }
}