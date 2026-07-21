namespace MudanzasWMS.backend.Models;

public class Entrada
{
    public int IdEntrada { get; set; }
    public int IdSku { get; set; }
    public int IdCliente { get; set; }
    public int IdUbicacion { get; set; }
    public int IdUsuario { get; set; }
    public int Cantidad { get; set; }
    public DateTime FechaIngreso { get; set; }
    public DateTime FechaRegistro { get; set; }
}
