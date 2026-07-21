using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MudanzasWMS.backend.Common;
using MudanzasWMS.backend.Models;
using MudanzasWMS.backend.Repositories;

namespace MudanzasWMS.backend.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly UsuarioRepository _usuarios;
    private readonly RefreshTokenRepository _refreshTokens;
    private readonly IConfiguration _config;

    public AuthService(
        IUnitOfWork uow,
        UsuarioRepository usuarios,
        RefreshTokenRepository refreshTokens,
        IConfiguration config)
    {
        _uow = uow;
        _usuarios = usuarios;
        _refreshTokens = refreshTokens;
        _config = config;
    }

    public async Task<LoginAttemptResult> LoginAsync(string email, string password)
    {
        await _uow.EnsureLoginStageAsync();
        var usuario = await _usuarios.FindByEmailForLoginAsync(email);
        if (usuario is null || !usuario.Activo) return LoginAttemptResult.Failed();

        if (usuario.BloqueadoHasta is { } bloqueadoHasta && bloqueadoHasta > DateTime.UtcNow)
            return LoginAttemptResult.LockedOutResult(bloqueadoHasta);

        if (!BCrypt.Net.BCrypt.Verify(password, usuario.ContrasenaHash))
        {
            var maxIntentos = _config.GetValue("Auth:MaxIntentosFallidos", 5);
            var intentos = await _usuarios.IncrementIntentosFallidosAsync(usuario.IdUsuario);

            if (intentos >= maxIntentos)
            {
                var minutos = _config.GetValue("Auth:BloqueoMinutos", 15);
                var bloqueadoHastaUtc = DateTime.UtcNow.AddMinutes(minutos);
                await _usuarios.BloquearAsync(usuario.IdUsuario, bloqueadoHastaUtc);
                return LoginAttemptResult.LockedOutResult(bloqueadoHastaUtc);
            }

            return LoginAttemptResult.Failed();
        }

        // Correct password: clear counter/lockout and proceed
        await _usuarios.ResetIntentosFallidosAsync(usuario.IdUsuario);
        await _usuarios.UpdateUltimoAccesoAsync(usuario.IdUsuario);
        var tokens = await IssueTokensAsync(usuario);
        return LoginAttemptResult.Success(tokens);
    }

    public async Task<LoginResult?> RefreshAsync(string refreshToken)
    {
        await _uow.EnsureLoginStageAsync();
        var hash = Hash(refreshToken);
        var stored = await _refreshTokens.FindValidByHashAsync(hash);
        if (stored is null) return null;

        var usuario = await _usuarios.FindByIdForLoginAsync(stored.IdUsuario);
        if (usuario is null || !usuario.Activo) return null;

        // Rotation: the old refresh token is single-use
        await _refreshTokens.RevokeAsync(stored.IdToken);
        return await IssueTokensAsync(usuario);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        await _uow.EnsureLoginStageAsync();
        var hash = Hash(refreshToken);
        var stored = await _refreshTokens.FindValidByHashAsync(hash);
        if (stored is not null)
        {
            await _refreshTokens.RevokeAsync(stored.IdToken);
        }
    }

    private async Task<LoginResult> IssueTokensAsync(Usuario usuario)
    {
        var (accessToken, expiresAt) = GenerateAccessToken(usuario);

        var refreshPlain = GenerateRefreshTokenValue();
        var refreshDays = _config.GetValue("Jwt:RefreshTokenDays", 7);
        await _refreshTokens.CreateAsync(usuario.IdUsuario, Hash(refreshPlain), DateTime.UtcNow.AddDays(refreshDays));

        return new LoginResult(accessToken, refreshPlain, expiresAt, usuario.IdUsuario, usuario.Rol.ToString(), usuario.NombreCompleto);
    }

    private (string token, DateTime expiresAtUtc) GenerateAccessToken(Usuario usuario)
    {
        var minutes = _config.GetValue("Jwt:AccessTokenMinutes", 1);
        var expiresAt = DateTime.UtcNow.AddMinutes(minutes);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
            new Claim(ClaimTypes.Role, usuario.Rol.ToString()),
            new Claim(ClaimTypes.Email, usuario.CorreoInstitucional),
            new Claim(ClaimTypes.Name, usuario.NombreCompleto)
        };

        var keyBytes = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);
        var creds = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    private static string GenerateRefreshTokenValue() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    private static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}