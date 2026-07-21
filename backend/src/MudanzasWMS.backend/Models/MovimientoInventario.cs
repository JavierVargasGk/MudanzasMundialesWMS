namespace MudanzasWMS.backend.Models;


public class MovimientoInventario
{
    public string TipoMovimiento { get; set; } = string.Empty; 
    public int IdReferencia { get; set; }
    public int IdSku { get; set; }
    public string CodigoSku { get; set; } = string.Empty;
    public int Cantidad { get; set; } 
    public DateTime Fecha { get; set; }
    public int? IdUsuario { get; set; }
    public string? Detalle { get; set; }
}
