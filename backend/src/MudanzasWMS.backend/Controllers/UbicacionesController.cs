using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MudanzasWMS.backend.Common;
using MudanzasWMS.backend.Models;
using MudanzasWMS.backend.Repositories;

namespace MudanzasWMS.backend.Controllers;

[ApiController]
[Route("api/ubicaciones")]
[Authorize]
public class UbicacionesController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly RequestContext _ctx;
    private readonly UbicacionRepository _ubicaciones;

    public UbicacionesController(IUnitOfWork uow, RequestContext ctx, UbicacionRepository ubicaciones)
    {
        _uow = uow;
        _ctx = ctx;
        _ubicaciones = ubicaciones;
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        await _uow.EnsureOpenAsync(_ctx);
        return Ok(await _ubicaciones.ListAsync());
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Create([FromBody] Ubicacion ubicacion)
    {
        await _uow.EnsureOpenAsync(_ctx);
        var id = await _ubicaciones.CreateAsync(ubicacion);
        return CreatedAtAction(nameof(List), new { id }, new { idUbicacion = id });
    }
}
