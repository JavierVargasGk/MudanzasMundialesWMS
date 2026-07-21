using MudanzasWMS.backend.Models.Enums;

namespace MudanzasWMS.backend.Models;

public class OrdenDespacho
{
    public int IdDespacho { get; set; }
    public int IdSku { get; set; }
    public int IdCliente { get; set; }
    public int CantidadSolicitada { get; set; }
    public DateTime FechaSalida { get; set; }
    public EstadoDespacho Estado { get; set; }
    public int IdUsuarioCreacion { get; set; }
    public int? IdUsuarioConfirmacion { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaConfirmacion { get; set; }
}
