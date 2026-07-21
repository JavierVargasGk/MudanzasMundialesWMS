using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MudanzasWMS.backend.Common;
using MudanzasWMS.backend.Models.Enums;
using MudanzasWMS.backend.Repositories;

namespace MudanzasWMS.backend.Controllers;

public record CrearDiscrepanciaRequest(int IdSku, int IdUbicacion, TipoDiscrepancia TipoDiscrepancia, string Observacion);

[ApiController]
[Route("api/clientes/{idCliente:int}/discrepancias")]
[Authorize]
public class DiscrepanciasController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly RequestContext _ctx;
    private readonly DiscrepanciaRepository _discrepancias;

    public DiscrepanciasController(IUnitOfWork uow, RequestContext ctx, DiscrepanciaRepository discrepancias)
    {
        _uow = uow;
        _ctx = ctx;
        _discrepancias = discrepancias;
    }

    [HttpGet]
    public async Task<IActionResult> List(int idCliente, [FromQuery] bool soloAbiertos = false)
    {
        await _uow.EnsureOpenAsync(_ctx, idCliente);
        return Ok(await _discrepancias.ListByClienteAsync(idCliente, soloAbiertos));
    }

    [HttpPost]
    public async Task<IActionResult> Create(int idCliente, [FromBody] CrearDiscrepanciaRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Observacion))
            return BadRequest(new { message = "La observacion es obligatoria." });

        await _uow.EnsureOpenAsync(_ctx, idCliente);
        var idReporte = await _discrepancias.CreateAsync(
            request.IdSku, idCliente, request.IdUbicacion, _ctx.UserId,
            request.TipoDiscrepancia, request.Observacion);

        return CreatedAtAction(nameof(List), new { idCliente }, new { idReporte });
    }

    [HttpPatch("{id:int}/descartar")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Descartar(int idCliente, int id)
    {
        await _uow.EnsureOpenAsync(_ctx, idCliente);
        await _discrepancias.DescartarAsync(id);
        return NoContent();
    }
}
