using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MudanzasWMS.backend.Services;

namespace MudanzasWMS.backend.Controllers;

public record LoginRequest(string Correo, string Contrasena);
public record RefreshRequest(string? RefreshToken);

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Endpoint to test authentication status.
    /// Returns 200 OK if access token is valid, 401 Unauthorized if missing/expired.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        // Extracts claims attached by JwtBearerHandler
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                  ?? User.FindFirst("sub")?.Value;
        
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value 
                ?? User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;

        return Ok(new
        {
            userId,
            role,
            isAuthenticated = true
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Correo) || string.IsNullOrWhiteSpace(request.Contrasena))
            return BadRequest(new { message = "Correo y contraseña son obligatorios." });

        var result = await _authService.LoginAsync(request.Correo, request.Contrasena);

        if (result.LockedOut)
        {
            return StatusCode(StatusCodes.Status423Locked, new
            {
                message = "Cuenta bloqueada temporalmente por demasiados intentos fallidos.",
                bloqueadoHastaUtc = result.LockedUntilUtc
            });
        }

        if (!result.Succeeded || result.Tokens is null)
            return Unauthorized(new { message = "Credenciales inválidas o usuario inactivo." });

        SetTokenCookie("accessToken", result.Tokens.AccessToken, result.Tokens.AccessTokenExpiresAtUtc);
        SetTokenCookie("refreshToken", result.Tokens.RefreshToken, DateTime.UtcNow.AddDays(7));

        return Ok(new
        {
            userId = result.Tokens.UserId,
            role = result.Tokens.Role,
            nombreCompleto = result.Tokens.NombreCompleto
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var refreshToken = Request.Cookies["refreshToken"] ?? request.RefreshToken;

        if (string.IsNullOrWhiteSpace(refreshToken))
            return BadRequest(new { message = "refreshToken es obligatorio." });

        var result = await _authService.RefreshAsync(refreshToken);
        if (result is null)
        {
            DeleteTokenCookie("accessToken");
            DeleteTokenCookie("refreshToken");
            return Unauthorized(new { message = "Refresh token inválido, revocado o expirado." });
        }

        SetTokenCookie("accessToken", result.AccessToken, result.AccessTokenExpiresAtUtc);
        SetTokenCookie("refreshToken", result.RefreshToken, DateTime.UtcNow.AddDays(7));

        return Ok(new
        {
            userId = result.UserId,
            role = result.Role,
            nombreCompleto = result.NombreCompleto
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest request)
    {
        var refreshToken = Request.Cookies["refreshToken"] ?? request.RefreshToken;

        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            await _authService.LogoutAsync(refreshToken);
        }

        DeleteTokenCookie("accessToken");
        DeleteTokenCookie("refreshToken");

        return NoContent();
    }

    private void SetTokenCookie(string name, string token, DateTime expiresUtc)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false, 
            SameSite = SameSiteMode.Lax,
            Expires = expiresUtc,
            Path = "/"
        };

        Response.Cookies.Append(name, token, cookieOptions);
    }

    private void DeleteTokenCookie(string name)
    {
        Response.Cookies.Delete(name, new CookieOptions
        {
            HttpOnly = true,
            Path = "/"
        });
    }
}