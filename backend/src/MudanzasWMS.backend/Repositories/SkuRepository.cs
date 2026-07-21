using Dapper;
using MudanzasWMS.backend.Common;
using MudanzasWMS.backend.Models;

namespace MudanzasWMS.backend.Repositories;

public class SkuRepository
{
    private readonly IUnitOfWork _uow;
    public SkuRepository(IUnitOfWork uow) => _uow = uow;

    private const string BaseSelect =
        "SELECT id_sku, id_cliente, codigo_sku, nombre, descripcion, unidad_medida, " +
        "fecha_limite_almacenamiento, activo, fecha_creacion FROM skus ";

    public Task<IEnumerable<Sku>> ListByClienteAsync(int idCliente, bool soloActivos = true) =>
        _uow.Connection.QueryAsync<Sku>(
            BaseSelect + "WHERE id_cliente = @idCliente " + (soloActivos ? "AND activo = TRUE " : "") + "ORDER BY codigo_sku",
            new { idCliente });

    public Task<Sku?> FindByIdAsync(int idSku) =>
        _uow.Connection.QuerySingleOrDefaultAsync<Sku>(BaseSelect + "WHERE id_sku = @idSku", new { idSku });

    public Task<IEnumerable<Sku>> ListProximosAVencerAsync(int idCliente, int dias) =>
        _uow.Connection.QueryAsync<Sku>(
            BaseSelect + "WHERE id_cliente = @idCliente AND activo = TRUE " +
            "AND fecha_limite_almacenamiento IS NOT NULL " +
            "AND fecha_limite_almacenamiento <= (CURRENT_DATE + (@dias || ' days')::interval) " +
            "ORDER BY fecha_limite_almacenamiento",
            new { idCliente, dias });

    public Task<int> CreateAsync(Sku sku) =>
        _uow.Connection.ExecuteScalarAsync<int>(
            "INSERT INTO skus (id_cliente, codigo_sku, nombre, descripcion, unidad_medida, fecha_limite_almacenamiento) " +
            "VALUES (@IdCliente, @CodigoSku, @Nombre, @Descripcion, @UnidadMedida, @FechaLimiteAlmacenamiento) " +
            "RETURNING id_sku",
            sku);

    public Task UpdateAsync(Sku sku) =>
        _uow.Connection.ExecuteAsync(
            "UPDATE skus SET nombre=@Nombre, descripcion=@Descripcion, unidad_medida=@UnidadMedida, " +
            "fecha_limite_almacenamiento=@FechaLimiteAlmacenamiento WHERE id_sku=@IdSku AND id_cliente=@IdCliente",
            sku);

    public Task SetActivoAsync(int idSku, int idCliente, bool activo) =>
        _uow.Connection.ExecuteAsync(
            "UPDATE skus SET activo=@activo WHERE id_sku=@idSku AND id_cliente=@idCliente",
            new { idSku, idCliente, activo });
}
