using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MudanzasWMS.backend.Common;
using MudanzasWMS.backend.Models;
using MudanzasWMS.backend.Repositories;

namespace MudanzasWMS.backend.Controllers;

[ApiController]
[Route("api/clientes/{idCliente:int}/skus")]
[Authorize]
public class SkusController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly RequestContext _ctx;
    private readonly SkuRepository _skus;

    public SkusController(IUnitOfWork uow, RequestContext ctx, SkuRepository skus)
    {
        _uow = uow;
        _ctx = ctx;
        _skus = skus;
    }
    [HttpGet]
    public async Task<IActionResult> List(int idCliente)
    {
        await _uow.EnsureOpenAsync(_ctx, idCliente);
        return Ok(await _skus.ListByClienteAsync(idCliente));
    }

    [HttpGet("{idSku:int}")]
    public async Task<IActionResult> Get(int idCliente, int idSku)
    {
        await _uow.EnsureOpenAsync(_ctx, idCliente);
        var sku = await _skus.FindByIdAsync(idSku);
        return sku is null ? NotFound() : Ok(sku);
    }

    [HttpGet("proximos-a-vencer")]
    public async Task<IActionResult> ProximosAVencer(int idCliente, [FromQuery] int dias = 15)
    {
        await _uow.EnsureOpenAsync(_ctx, idCliente);
        return Ok(await _skus.ListProximosAVencerAsync(idCliente, dias));
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Create(int idCliente, [FromBody] Sku sku)
    {
        sku.IdCliente = idCliente;
        await _uow.EnsureOpenAsync(_ctx, idCliente);
        var id = await _skus.CreateAsync(sku);
        return CreatedAtAction(nameof(Get), new { idCliente, idSku = id }, new { idSku = id });
    }

    [HttpPut("{idSku:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Update(int idCliente, int idSku, [FromBody] Sku sku)
    {
        sku.IdSku = idSku;
        sku.IdCliente = idCliente;
        await _uow.EnsureOpenAsync(_ctx, idCliente);
        await _skus.UpdateAsync(sku);
        return NoContent();
    }

    [HttpPatch("{idSku:int}/activo")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> SetActivo(int idCliente, int idSku, [FromBody] SetActivoRequest request)
    {
        await _uow.EnsureOpenAsync(_ctx, idCliente);
        await _skus.SetActivoAsync(idSku, idCliente, request.Activo);
        return NoContent();
    }
}
