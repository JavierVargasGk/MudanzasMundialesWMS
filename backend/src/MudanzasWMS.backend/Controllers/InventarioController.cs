using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MudanzasWMS.backend.Common;
using MudanzasWMS.backend.Repositories;

namespace MudanzasWMS.backend.Controllers;

[ApiController]
[Route("api/clientes/{idCliente:int}/inventario")]
[Authorize]
public class InventarioController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly RequestContext _ctx;
    private readonly StockRepository _stock;
    private readonly MovimientoRepository _movimientos;

    public InventarioController(IUnitOfWork uow, RequestContext ctx, StockRepository stock, MovimientoRepository movimientos)
    {
        _uow = uow;
        _ctx = ctx;
        _stock = stock;
        _movimientos = movimientos;
    }

    [HttpGet("stock")]
    public async Task<IActionResult> Stock(int idCliente)
    {
        await _uow.EnsureOpenAsync(_ctx, idCliente);
        return Ok(await _stock.ListByClienteAsync(idCliente));
    }

    [HttpGet("movimientos")]
    public async Task<IActionResult> Movimientos(int idCliente, [FromQuery] int? idSku)
    {
        await _uow.EnsureOpenAsync(_ctx, idCliente);
        return Ok(await _movimientos.ListByClienteAsync(idCliente, idSku));
    }
}
