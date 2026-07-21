namespace MudanzasWMS.backend.Common;


public class UnitOfWorkMiddleware
{
    private readonly RequestDelegate _next;

    public UnitOfWorkMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, IUnitOfWork unitOfWork)
    {
        try
        {
            await _next(httpContext);
            if (httpContext.Response.StatusCode < 400)
                await unitOfWork.CommitAsync();
            else
                await unitOfWork.RollbackAsync();
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
        finally
        {
            await unitOfWork.DisposeAsync();
        }
    }
}
