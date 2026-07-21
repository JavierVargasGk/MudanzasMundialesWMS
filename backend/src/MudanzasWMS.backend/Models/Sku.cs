namespace MudanzasWMS.backend.Models;

public class Sku
{
    public int IdSku { get; set; }
    public int IdCliente { get; set; }
    public string CodigoSku { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string UnidadMedida { get; set; } = string.Empty;
    public DateTime? FechaLimiteAlmacenamiento { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}
