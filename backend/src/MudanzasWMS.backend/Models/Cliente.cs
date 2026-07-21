namespace MudanzasWMS.backend.Models;

public class Cliente
{
    public int IdCliente { get; set; }
    public string NombreEmpresa { get; set; } = string.Empty;
    public string? IdentificacionJuridica { get; set; }
    public string? ContactoNombre { get; set; }
    public string? ContactoTelefono { get; set; }
    public string? ContactoCorreo { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaRegistro { get; set; }
}
