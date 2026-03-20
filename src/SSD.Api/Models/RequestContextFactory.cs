using SSD.Application.Contracts.Auth;

namespace SSD.Api.Models;

public static class RequestContextFactory
{
    public static AuthRequestContext Create(HttpContext httpContext)
    {
        return new AuthRequestContext(
            httpContext.Connection.RemoteIpAddress?.ToString(),
            httpContext.Request.Headers["User-Agent"].ToString(),
            httpContext.TraceIdentifier);
    }
}
