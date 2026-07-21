using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MudanzasWMS.backend.Common;
using MudanzasWMS.backend.Repositories;

namespace MudanzasWMS.backend.Controllers;

public record CrearDespachoRequest(int IdSku, int CantidadSolicitada, DateTime FechaSalida);

[ApiController]
[Route("api/clientes/{idCliente:int}/despachos")]
[Authorize]
public class DespachosController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly RequestContext _ctx;
    private readonly DespachoRepository _despachos;
    private readonly StockRepository _stock;

    public DespachosController(IUnitOfWork uow, RequestContext ctx, DespachoRepository despachos, StockRepository stock)
    {
        _uow = uow;
        _ctx = ctx;
        _despachos = despachos;
        _stock = stock;
    }

    [HttpGet]
    public async Task<IActionResult> List(int idCliente)
    {
        await _uow.EnsureOpenAsync(_ctx, idCliente);
        return Ok(await _despachos.ListByClienteAsync(idCliente));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int idCliente, int id)
    {
        await _uow.EnsureOpenAsync(_ctx, idCliente);
        var despacho = await _despachos.FindByIdAsync(id);
        return despacho is null ? NotFound() : Ok(despacho);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Create(int idCliente, [FromBody] CrearDespachoRequest request)
    {
        if (request.CantidadSolicitada <= 0)
            return BadRequest(new { message = "La cantidad solicitada debe ser mayor a cero." });

        await _uow.EnsureOpenAsync(_ctx, idCliente);
        var disponible = await _stock.GetCantidadDisponibleForUpdateAsync(request.IdSku, idCliente);

        if (disponible < request.CantidadSolicitada)
        {
            var idPendiente = await _despachos.CreatePendienteAsync(
                request.IdSku, idCliente, request.CantidadSolicitada, request.FechaSalida, _ctx.UserId);
            return StatusCode(202, new { idDespacho = idPendiente, estado = "pendiente", disponible });
        }

        var idConfirmado = await _despachos.CreateConfirmadoAsync(
            request.IdSku, idCliente, request.CantidadSolicitada, request.FechaSalida, _ctx.UserId);
        await _stock.DecrementarAsync(request.IdSku, idCliente, request.CantidadSolicitada);

        return Ok(new { idDespacho = idConfirmado, estado = "confirmado" });
    }
}
