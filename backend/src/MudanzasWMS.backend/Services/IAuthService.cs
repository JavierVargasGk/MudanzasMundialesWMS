namespace MudanzasWMS.backend.Services;

public record LoginResult(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAtUtc,
    int UserId,
    string Role,
    string NombreCompleto);

public class LoginAttemptResult
{
    public bool Succeeded { get; }
    public bool LockedOut { get; }
    public DateTime? LockedUntilUtc { get; }
    public LoginResult? Tokens { get; }

    private LoginAttemptResult(bool succeeded, bool lockedOut, DateTime? lockedUntilUtc, LoginResult? tokens)
    {
        Succeeded = succeeded;
        LockedOut = lockedOut;
        LockedUntilUtc = lockedUntilUtc;
        Tokens = tokens;
    }

    public static LoginAttemptResult Success(LoginResult tokens) => new(true, false, null, tokens);
    public static LoginAttemptResult Failed() => new(false, false, null, null);
    public static LoginAttemptResult LockedOutResult(DateTime lockedUntilUtc) => new(false, true, lockedUntilUtc, null);
}

public interface IAuthService
{
    Task<LoginAttemptResult> LoginAsync(string email, string password);
    Task<LoginResult?> RefreshAsync(string refreshToken);
    Task LogoutAsync(string refreshToken);
}