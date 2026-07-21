using Dapper;
using MudanzasWMS.backend.Common;
using MudanzasWMS.backend.Models;

namespace MudanzasWMS.backend.Repositories;

public class RefreshTokenRepository
{
    private readonly IUnitOfWork _uow;

    public RefreshTokenRepository(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public Task CreateAsync(int idUsuario, string tokenHash, DateTime expiresAtUtc) =>
        _uow.Connection.ExecuteAsync(
            "INSERT INTO refresh_tokens (id_usuario, token_hash, fecha_expiracion) " +
            "VALUES (@idUsuario, @tokenHash, @expiresAtUtc)",
            new { idUsuario, tokenHash, expiresAtUtc });

    public Task<RefreshToken?> FindValidByHashAsync(string tokenHash) =>
        _uow.Connection.QuerySingleOrDefaultAsync<RefreshToken>(
            "SELECT id_token, id_usuario, token_hash, fecha_expiracion, revocado, fecha_creacion " +
            "FROM refresh_tokens WHERE token_hash = @tokenHash AND revocado = FALSE " +
            "AND fecha_expiracion > now()",
            new { tokenHash });

    public Task RevokeAsync(Guid idToken) =>
        _uow.Connection.ExecuteAsync(
            "UPDATE refresh_tokens SET revocado = TRUE WHERE id_token = @idToken",
            new { idToken });
}
