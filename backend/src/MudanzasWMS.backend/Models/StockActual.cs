namespace MudanzasWMS.backend.Models;

using MudanzasWMS.backend.Models.Enums;
using System.Text.Json.Serialization;
public class StockActual
{
    public int IdStock { get; set; }
    public int IdSku { get; set; }
    public int IdCliente { get; set; }
    public int IdUbicacion { get; set; }
    public int CantidadActual { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public string? CodigoSku { get; set; }
    public string? NombreSku { get; set; }
    public string? CodigoUbicacion { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))] 
    public EstadoLote Status { get; set; } = EstadoLote.Disponible;
    public string? NumeroLote { get; set; }
}