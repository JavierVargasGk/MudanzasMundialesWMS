using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MudanzasWMS.backend.Common;
using MudanzasWMS.backend.Models;
using MudanzasWMS.backend.Repositories;

namespace MudanzasWMS.backend.Controllers;

public record CrearEntradaRequest(int IdSku, int IdUbicacion, int Cantidad, DateTime FechaIngreso);

[ApiController]
[Route("api/clientes/{idCliente:int}/entradas")]
[Authorize]
public class EntradasController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly RequestContext _ctx;
    private readonly EntradaRepository _entradas;
    private readonly StockRepository _stock;

    public EntradasController(IUnitOfWork uow, RequestContext ctx, EntradaRepository entradas, StockRepository stock)
    {
        _uow = uow;
        _ctx = ctx;
        _entradas = entradas;
        _stock = stock;
    }

    [HttpGet]
    public async Task<IActionResult> List(int idCliente, [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
    {
        await _uow.EnsureOpenAsync(_ctx, idCliente);
        return Ok(await _entradas.ListByClienteAsync(idCliente, desde, hasta));
    }
    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Create(int idCliente, [FromBody] CrearEntradaRequest request)
    {
        if (request.Cantidad <= 0)
            return BadRequest(new { message = "La cantidad debe ser mayor a cero." });

        await _uow.EnsureOpenAsync(_ctx, idCliente);

        var entrada = new Entrada
        {
            IdSku = request.IdSku,
            IdCliente = idCliente,
            IdUbicacion = request.IdUbicacion,
            IdUsuario = _ctx.UserId,
            Cantidad = request.Cantidad,
            FechaIngreso = request.FechaIngreso
        };
        var idEntrada = await _entradas.CreateAsync(entrada);
        await _stock.IncrementarAsync(request.IdSku, idCliente, request.IdUbicacion, request.Cantidad);

        return CreatedAtAction(nameof(List), new { idCliente }, new { idEntrada });
    }
}
