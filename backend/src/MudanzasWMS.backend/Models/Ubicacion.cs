namespace MudanzasWMS.backend.Models;

public class Ubicacion
{
    public int IdUbicacion { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string? Pasillo { get; set; }
    public string? Estante { get; set; }
    public int? CapacidadMaxima { get; set; }
    public bool Activo { get; set; }
}
