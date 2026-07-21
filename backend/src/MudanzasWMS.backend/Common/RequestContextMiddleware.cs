using System.Security.Claims;

namespace MudanzasWMS.backend.Common;

public class RequestContextMiddleware
{
    private readonly RequestDelegate _next;

    public RequestContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, RequestContext requestContext)
    {
        var user = httpContext.User;
        if (user.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;
            if (userIdClaim is not null && roleClaim is not null && int.TryParse(userIdClaim, out var userId))
            {
                requestContext.Load(userId, roleClaim);
            }
        }

        await _next(httpContext);
    }
}
