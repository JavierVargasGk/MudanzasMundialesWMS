namespace MudanzasWMS.backend.Models;

public class RefreshToken
{
    public Guid IdToken { get; set; }
    public int IdUsuario { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime FechaExpiracion { get; set; }
    public bool Revocado { get; set; }
    public DateTime FechaCreacion { get; set; }
}
