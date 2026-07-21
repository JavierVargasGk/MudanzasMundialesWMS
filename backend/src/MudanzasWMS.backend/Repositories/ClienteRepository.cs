using Dapper;
using MudanzasWMS.backend.Common;
using MudanzasWMS.backend.Models;

namespace MudanzasWMS.backend.Repositories;

public class ClienteRepository
{
    private readonly IUnitOfWork _uow;
    public ClienteRepository(IUnitOfWork uow) => _uow = uow;

    public Task<IEnumerable<Cliente>> ListAsync(bool soloActivos = true) =>
        _uow.Connection.QueryAsync<Cliente>(
            "SELECT id_cliente, nombre_empresa, identificacion_juridica, contacto_nombre, " +
            "contacto_telefono, contacto_correo, activo, fecha_registro FROM clientes " +
            (soloActivos ? "WHERE activo = TRUE " : "") + "ORDER BY nombre_empresa");

    public Task<Cliente?> FindByIdAsync(int idCliente) =>
        _uow.Connection.QuerySingleOrDefaultAsync<Cliente>(
            "SELECT id_cliente, nombre_empresa, identificacion_juridica, contacto_nombre, " +
            "contacto_telefono, contacto_correo, activo, fecha_registro FROM clientes WHERE id_cliente = @idCliente",
            new { idCliente });

    public Task<int> CreateAsync(Cliente cliente) =>
        _uow.Connection.ExecuteScalarAsync<int>(
            "INSERT INTO clientes (nombre_empresa, identificacion_juridica, contacto_nombre, contacto_telefono, contacto_correo) " +
            "VALUES (@NombreEmpresa, @IdentificacionJuridica, @ContactoNombre, @ContactoTelefono, @ContactoCorreo) RETURNING id_cliente",
            cliente);

    public Task UpdateAsync(Cliente cliente) =>
        _uow.Connection.ExecuteAsync(
            "UPDATE clientes SET nombre_empresa=@NombreEmpresa, identificacion_juridica=@IdentificacionJuridica, " +
            "contacto_nombre=@ContactoNombre, contacto_telefono=@ContactoTelefono, contacto_correo=@ContactoCorreo " +
            "WHERE id_cliente=@IdCliente",
            cliente);

    public Task SetActivoAsync(int idCliente, bool activo) =>
        _uow.Connection.ExecuteAsync(
            "UPDATE clientes SET activo=@activo WHERE id_cliente=@idCliente",
            new { idCliente, activo });
}
