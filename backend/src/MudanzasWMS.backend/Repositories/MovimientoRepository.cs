using Dapper;
using MudanzasWMS.backend.Common;
using MudanzasWMS.backend.Models;

namespace MudanzasWMS.backend.Repositories;

public class MovimientoRepository
{
    private readonly IUnitOfWork _uow;
    public MovimientoRepository(IUnitOfWork uow) => _uow = uow;

    public Task<IEnumerable<MovimientoInventario>> ListByClienteAsync(int idCliente, int? idSku = null)
    {
        const string sql = @"
            SELECT 'entrada' AS tipo_movimiento, e.id_entrada AS id_referencia, e.id_sku,
                   k.codigo_sku, e.cantidad AS cantidad, e.fecha_registro AS fecha,
                   e.id_usuario, 'Entrada de mercancia' AS detalle
            FROM entradas e JOIN skus k ON k.id_sku = e.id_sku
            WHERE e.id_cliente = @idCliente AND (@idSku::int IS NULL OR e.id_sku = @idSku)

            UNION ALL

            SELECT 'despacho', d.id_despacho, d.id_sku,
                   k.codigo_sku, -d.cantidad_solicitada, COALESCE(d.fecha_confirmacion, d.fecha_creacion),
                   d.id_usuario_confirmacion, 'Despacho confirmado'
            FROM ordenes_despacho d JOIN skus k ON k.id_sku = d.id_sku
            WHERE d.id_cliente = @idCliente AND d.estado = 'confirmado'
              AND (@idSku::int IS NULL OR d.id_sku = @idSku)

            UNION ALL

            SELECT 'ajuste', a.id_ajuste, a.id_sku,
                   k.codigo_sku, a.cantidad_ajuste, a.fecha_registro,
                   a.id_usuario_solicitante, a.tipo_ajuste::text || ': ' || a.razon
            FROM ajustes_inventario a JOIN skus k ON k.id_sku = a.id_sku
            WHERE a.id_cliente = @idCliente AND (@idSku::int IS NULL OR a.id_sku = @idSku)

            ORDER BY fecha DESC";

        return _uow.Connection.QueryAsync<MovimientoInventario>(sql, new { idCliente, idSku });
    }
}
