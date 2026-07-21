namespace MudanzasWMS.backend.Common;


public class RequestContext
{
    public int UserId { get; private set; }
    public string Role { get; private set; } = string.Empty;
    public bool IsAuthenticated { get; private set; }
    public int? ClienteId { get; set; }

    public void Load(int userId, string role)
    {
        UserId = userId;
        Role = role;
        IsAuthenticated = true;
    }
}
