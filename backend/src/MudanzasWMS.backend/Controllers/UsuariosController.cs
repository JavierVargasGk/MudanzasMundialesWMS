using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MudanzasWMS.backend.Common;
using MudanzasWMS.backend.Models.Enums;
using MudanzasWMS.backend.Repositories;

namespace MudanzasWMS.backend.Controllers;

public record CrearUsuarioRequest(string NombreCompleto, string Correo, string Contrasena, RolUsuario Rol);
public record CambiarPasswordRequest(string ContrasenaActual, string ContrasenaNueva);
public record SetActivoRequest(bool Activo);

[ApiController]
[Route("api/usuarios")]
[Authorize]
public class UsuariosController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly RequestContext _ctx;
    private readonly UsuarioRepository _usuarios;

    public UsuariosController(IUnitOfWork uow, RequestContext ctx, UsuarioRepository usuarios)
    {
        _uow = uow;
        _ctx = ctx;
        _usuarios = usuarios;
    }

    // RF-U1: Administrador crea/edita/desactiva usuarios
    [HttpGet]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> List()
    {
        await _uow.EnsureOpenAsync(_ctx);
        return Ok(await _usuarios.ListAsync());
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Create([FromBody] CrearUsuarioRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Contrasena) || request.Contrasena.Length < 8)
            return BadRequest(new { message = "La contrasena debe tener al menos 8 caracteres." });

        await _uow.EnsureOpenAsync(_ctx);
        var hash = BCrypt.Net.BCrypt.HashPassword(request.Contrasena, workFactor: 12);
        var id = await _usuarios.CreateAsync(request.NombreCompleto, request.Correo, hash, request.Rol);
        return CreatedAtAction(nameof(List), new { id }, new { idUsuario = id });
    }

    [HttpPatch("{id:int}/activo")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> SetActivo(int id, [FromBody] SetActivoRequest request)
    {
        await _uow.EnsureOpenAsync(_ctx);
        await _usuarios.SetActivoAsync(id, request.Activo);
        return NoContent();
    }

    [HttpPost("{id:int}/desbloquear")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Desbloquear(int id)
    {
        await _uow.EnsureOpenAsync(_ctx);
        await _usuarios.DesbloquearAsync(id);
        return NoContent();
    }

    [HttpPost("mi-cuenta/cambiar-password")]
    public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordRequest request)
    {
        await _uow.EnsureOpenAsync(_ctx);
        var usuario = await _usuarios.FindByIdWithHashAsync(_ctx.UserId);
        if (usuario is null || !BCrypt.Net.BCrypt.Verify(request.ContrasenaActual, usuario.ContrasenaHash))
            return BadRequest(new { message = "La contrasena actual no es correcta." });

        if (string.IsNullOrWhiteSpace(request.ContrasenaNueva) || request.ContrasenaNueva.Length < 8)
            return BadRequest(new { message = "La nueva contrasena debe tener al menos 8 caracteres." });

        var newHash = BCrypt.Net.BCrypt.HashPassword(request.ContrasenaNueva, workFactor: 12);
        await _usuarios.UpdatePasswordAsync(_ctx.UserId, newHash);
        return NoContent();
    }
}