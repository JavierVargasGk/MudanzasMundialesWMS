using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MudanzasWMS.backend.Common;
using MudanzasWMS.backend.Models.Enums;
using MudanzasWMS.backend.Repositories;

namespace MudanzasWMS.backend.Controllers;

public record CrearAjusteRequest(
    int IdSku, int IdUbicacion, TipoAjuste TipoAjuste, int CantidadAjuste, string Razon,
    int? IdReporteDiscrepancia = null);

[ApiController]
[Route("api/clientes/{idCliente:int}/ajustes")]
[Authorize(Roles = "Administrador")]
public class AjustesController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly RequestContext _ctx;
    private readonly AjusteRepository _ajustes;
    private readonly StockRepository _stock;
    private readonly DiscrepanciaRepository _discrepancias;

    public AjustesController(
        IUnitOfWork uow, RequestContext ctx, AjusteRepository ajustes,
        StockRepository stock, DiscrepanciaRepository discrepancias)
    {
        _uow = uow;
        _ctx = ctx;
        _ajustes = ajustes;
        _stock = stock;
        _discrepancias = discrepancias;
    }

    [HttpGet]
    public async Task<IActionResult> List(int idCliente)
    {
        await _uow.EnsureOpenAsync(_ctx, idCliente);
        return Ok(await _ajustes.ListByClienteAsync(idCliente));
    }

    [HttpPost]
    public async Task<IActionResult> Create(int idCliente, [FromBody] CrearAjusteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Razon))
            return BadRequest(new { message = "La razon del ajuste es obligatoria (RF-I2)." });

        await _uow.EnsureOpenAsync(_ctx, idCliente);

        var idAjuste = await _ajustes.CreateAsync(
            request.IdSku, idCliente, request.IdUbicacion, _ctx.UserId,
            request.TipoAjuste, request.CantidadAjuste, request.Razon);

        await _stock.AplicarAjusteAsync(request.IdSku, idCliente, request.IdUbicacion, request.CantidadAjuste);

        if (request.IdReporteDiscrepancia is { } idReporte)
        {
            await _discrepancias.ResolverAsync(idReporte, idAjuste);
        }

        return CreatedAtAction(nameof(List), new { idCliente }, new { idAjuste });
    }
}
