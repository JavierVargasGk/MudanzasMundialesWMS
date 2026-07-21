namespace MudanzasWMS.backend.Models;

public class Auditoria
{
    public long IdAuditoria { get; set; }
    public int? IdUsuario { get; set; }
    public string TablaAfectada { get; set; } = string.Empty;
    public int IdEntidadAfectada { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string? ValoresAnteriores { get; set; } // raw JSON text
    public string? ValoresNuevos { get; set; }     // raw JSON text
    public DateTime Fecha { get; set; }
}
