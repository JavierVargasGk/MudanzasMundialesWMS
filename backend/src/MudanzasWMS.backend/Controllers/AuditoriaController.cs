using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MudanzasWMS.backend.Common;
using MudanzasWMS.backend.Repositories;

namespace MudanzasWMS.backend.Controllers;

[ApiController]
[Route("api/auditoria")]
[Authorize(Roles = "Administrador")]
public class AuditoriaController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly RequestContext _ctx;
    private readonly AuditoriaRepository _auditoria;

    public AuditoriaController(IUnitOfWork uow, RequestContext ctx, AuditoriaRepository auditoria)
    {
        _uow = uow;
        _ctx = ctx;
        _auditoria = auditoria;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? tablaAfectada, [FromQuery] int take = 200)
    {
        await _uow.EnsureOpenAsync(_ctx);
        return Ok(await _auditoria.ListAsync(tablaAfectada, take));
    }
}
