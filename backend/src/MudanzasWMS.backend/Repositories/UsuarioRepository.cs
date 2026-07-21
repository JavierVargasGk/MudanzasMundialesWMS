using Dapper;
using MudanzasWMS.backend.Common;
using MudanzasWMS.backend.Models;

namespace MudanzasWMS.backend.Repositories;

public class UsuarioRepository
{
    private readonly IUnitOfWork _uow;

    public UsuarioRepository(IUnitOfWork uow)
    {
        _uow = uow;
    }


    public Task<Usuario?> FindByEmailForLoginAsync(string email) =>
        _uow.Connection.QuerySingleOrDefaultAsync<Usuario>(
            "SELECT id_usuario AS IdUsuario, " +
            "nombre_completo AS NombreCompleto, " +
            "correo_institucional AS CorreoInstitucional, " +
            "contrasena_hash AS ContrasenaHash, " +
            "rol::text AS Rol, " +
            "activo AS Activo, " +
            "fecha_creacion AS FechaCreacion, " +
            "ultimo_acceso AS UltimoAcceso, " +
            "intentos_fallidos AS IntentosFallidos, " +
            "bloqueado_hasta AS BloqueadoHasta " +
            "FROM usuarios WHERE correo_institucional = @email",
            new { email });

    public Task<Usuario?> FindByIdForLoginAsync(int idUsuario) =>
        _uow.Connection.QuerySingleOrDefaultAsync<Usuario>(
            "SELECT id_usuario, nombre_completo, correo_institucional, contrasena_hash, " +
            "rol::text AS rol, activo, fecha_creacion, ultimo_acceso, intentos_fallidos, bloqueado_hasta " +
            "FROM usuarios WHERE id_usuario = @idUsuario",
            new { idUsuario });

    public Task UpdateUltimoAccesoAsync(int idUsuario) =>
        _uow.Connection.ExecuteAsync(
            "UPDATE usuarios SET ultimo_acceso = now() WHERE id_usuario = @idUsuario",
            new { idUsuario });


    public Task<IEnumerable<Usuario>> ListAsync() =>
        _uow.Connection.QueryAsync<Usuario>(
            "SELECT id_usuario, nombre_completo, correo_institucional, rol::text AS rol, activo, " +
            "fecha_creacion, ultimo_acceso, intentos_fallidos, bloqueado_hasta " +
            "FROM usuarios ORDER BY nombre_completo");

    public Task<Usuario?> FindByIdAsync(int idUsuario) =>
        _uow.Connection.QuerySingleOrDefaultAsync<Usuario>(
            "SELECT id_usuario, nombre_completo, correo_institucional, rol::text AS rol, activo, " +
            "fecha_creacion, ultimo_acceso, intentos_fallidos, bloqueado_hasta " +
            "FROM usuarios WHERE id_usuario = @idUsuario",
            new { idUsuario });


    public Task<Usuario?> FindByIdWithHashAsync(int idUsuario) =>
        _uow.Connection.QuerySingleOrDefaultAsync<Usuario>(
            "SELECT id_usuario, nombre_completo, correo_institucional, contrasena_hash, " +
            "rol::text AS rol, activo, fecha_creacion, ultimo_acceso FROM usuarios WHERE id_usuario = @idUsuario",
            new { idUsuario });

    public Task<int> CreateAsync(string nombreCompleto, string correo, string hash, Models.Enums.RolUsuario rol) =>
        _uow.Connection.ExecuteScalarAsync<int>(
            "INSERT INTO usuarios (nombre_completo, correo_institucional, contrasena_hash, rol) " +
            "VALUES (@nombreCompleto, @correo, @hash, @rol::rol_usuario) RETURNING id_usuario",
            new { nombreCompleto, correo, hash, rol = rol.ToString() });

    public Task SetActivoAsync(int idUsuario, bool activo) =>
        _uow.Connection.ExecuteAsync(
            "UPDATE usuarios SET activo = @activo WHERE id_usuario = @idUsuario",
            new { idUsuario, activo });

    public Task UpdatePasswordAsync(int idUsuario, string newHash) =>
        _uow.Connection.ExecuteAsync(
            "UPDATE usuarios SET contrasena_hash = @newHash WHERE id_usuario = @idUsuario",
            new { idUsuario, newHash });

    public Task<int> IncrementIntentosFallidosAsync(int idUsuario) =>
        _uow.Connection.ExecuteScalarAsync<int>(
            "UPDATE usuarios SET intentos_fallidos = intentos_fallidos + 1 " +
            "WHERE id_usuario = @idUsuario RETURNING intentos_fallidos",
            new { idUsuario });


    public Task BloquearAsync(int idUsuario, DateTime bloqueadoHastaUtc) =>
        _uow.Connection.ExecuteAsync(
            "UPDATE usuarios SET bloqueado_hasta = @bloqueadoHastaUtc WHERE id_usuario = @idUsuario",
            new { idUsuario, bloqueadoHastaUtc });

    public Task ResetIntentosFallidosAsync(int idUsuario) =>
        _uow.Connection.ExecuteAsync(
            "UPDATE usuarios SET intentos_fallidos = 0, bloqueado_hasta = NULL WHERE id_usuario = @idUsuario",
            new { idUsuario });
    public Task DesbloquearAsync(int idUsuario) => ResetIntentosFallidosAsync(idUsuario);
}