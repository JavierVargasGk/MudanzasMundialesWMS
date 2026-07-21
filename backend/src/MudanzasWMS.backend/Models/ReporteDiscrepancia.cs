using MudanzasWMS.backend.Models.Enums;

namespace MudanzasWMS.backend.Models;

public class ReporteDiscrepancia
{
    public int IdReporte { get; set; }
    public int IdSku { get; set; }
    public int IdCliente { get; set; }
    public int IdUbicacion { get; set; }
    public int IdUsuarioReporta { get; set; }
    public TipoDiscrepancia TipoDiscrepancia { get; set; }
    public string Observacion { get; set; } = string.Empty;
    public EstadoDiscrepancia Estado { get; set; }
    public int? IdAjusteResultante { get; set; }
    public DateTime FechaReporte { get; set; }
    public DateTime? FechaResolucion { get; set; }
}
