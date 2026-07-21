using Dapper;
using MudanzasWMS.backend.Common;
using MudanzasWMS.backend.Models;

namespace MudanzasWMS.backend.Repositories;

public class UbicacionRepository
{
    private readonly IUnitOfWork _uow;
    public UbicacionRepository(IUnitOfWork uow) => _uow = uow;

    public Task<IEnumerable<Ubicacion>> ListAsync(bool soloActivas = true) =>
        _uow.Connection.QueryAsync<Ubicacion>(
            "SELECT id_ubicacion, codigo, pasillo, estante, capacidad_maxima, activo FROM ubicaciones " +
            (soloActivas ? "WHERE activo = TRUE " : "") + "ORDER BY codigo");

    public Task<Ubicacion?> FindByIdAsync(int idUbicacion) =>
        _uow.Connection.QuerySingleOrDefaultAsync<Ubicacion>(
            "SELECT id_ubicacion, codigo, pasillo, estante, capacidad_maxima, activo " +
            "FROM ubicaciones WHERE id_ubicacion = @idUbicacion",
            new { idUbicacion });

    public Task<int> CreateAsync(Ubicacion ubicacion) =>
        _uow.Connection.ExecuteScalarAsync<int>(
            "INSERT INTO ubicaciones (codigo, pasillo, estante, capacidad_maxima) " +
            "VALUES (@Codigo, @Pasillo, @Estante, @CapacidadMaxima) RETURNING id_ubicacion",
            ubicacion);
}
