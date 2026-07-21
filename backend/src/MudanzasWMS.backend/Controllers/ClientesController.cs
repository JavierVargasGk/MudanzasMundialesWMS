using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MudanzasWMS.backend.Common;
using MudanzasWMS.backend.Models;
using MudanzasWMS.backend.Repositories;

namespace MudanzasWMS.backend.Controllers;

[ApiController]
[Route("api/clientes")]
[Authorize]
public class ClientesController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly RequestContext _ctx;
    private readonly ClienteRepository _clientes;

    public ClientesController(IUnitOfWork uow, RequestContext ctx, ClienteRepository clientes)
    {
        _uow = uow;
        _ctx = ctx;
        _clientes = clientes;
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        await _uow.EnsureOpenAsync(_ctx);
        return Ok(await _clientes.ListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        await _uow.EnsureOpenAsync(_ctx);
        var cliente = await _clientes.FindByIdAsync(id);
        return cliente is null ? NotFound() : Ok(cliente);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Create([FromBody] Cliente cliente)
    {
        await _uow.EnsureOpenAsync(_ctx);
        var id = await _clientes.CreateAsync(cliente);
        return CreatedAtAction(nameof(Get), new { id }, new { idCliente = id });
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Update(int id, [FromBody] Cliente cliente)
    {
        cliente.IdCliente = id;
        await _uow.EnsureOpenAsync(_ctx);
        await _clientes.UpdateAsync(cliente);
        return NoContent();
    }

    [HttpPatch("{id:int}/activo")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> SetActivo(int id, [FromBody] SetActivoRequest request)
    {
        await _uow.EnsureOpenAsync(_ctx);
        await _clientes.SetActivoAsync(id, request.Activo);
        return NoContent();
    }
}
