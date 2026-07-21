namespace MudanzasWMS.backend.Models.Enums;

/// <summary>
/// Matches the Postgres enum `rol_usuario`. Values are read/written as their
/// string name (see DapperTypeHandlers), never as integers, so the labels
/// here and in Postgres must stay identical.
/// </summary>
public enum RolUsuario
{
    Administrador,
    Analista,
    Operario
}
