using MudanzasWMS.backend.Models.Enums;

namespace MudanzasWMS.backend.Models;

public class AjusteInventario
{
    public int IdAjuste { get; set; }
    public int IdSku { get; set; }
    public int IdCliente { get; set; }
    public int IdUbicacion { get; set; }
    public int IdUsuarioSolicitante { get; set; }
    public TipoAjuste TipoAjuste { get; set; }
    public int CantidadAjuste { get; set; }
    public string Razon { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
}
