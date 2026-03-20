using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using SSD.Api.Models;
using SSD.Application.Abstractions;
using SSD.Application.Contracts.Auth;
using SSD.Application.Exceptions;
using SSD.Application.Contracts;
using SSD.Application.Contracts.Spotify;
using SSD.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 8,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1),
                AutoReplenishment = true
            }));
});

var signingKey = builder.Configuration["Security:JwtSigningKey"] ?? builder.Configuration["SSD_JWT_SIGNING_KEY"] ?? string.Empty;
var issuer = builder.Configuration["Security:JwtIssuer"] ?? builder.Configuration["SSD_JWT_ISSUER"] ?? "SSD.Api";
var audience = builder.Configuration["Security:JwtAudience"] ?? builder.Configuration["SSD_JWT_AUDIENCE"] ?? "SSD.Mobile";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler(handlerApp =>
{
    handlerApp.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionFeature?.Error;
        var correlationId = context.TraceIdentifier;

        if (exception is AuthException authException)
        {
            context.Response.StatusCode = authException.Code is "email_in_use"
                ? StatusCodes.Status409Conflict
                : StatusCodes.Status401Unauthorized;

            await context.Response.WriteAsJsonAsync(new ApiErrorResponse(
                authException.Code,
                authException.Message,
                authException.Errors,
                correlationId));
            return;
        }

        if (exception is IntegrationException integrationException)
        {
            context.Response.StatusCode = integrationException.StatusCode;
            await context.Response.WriteAsJsonAsync(new ApiErrorResponse(
                integrationException.Code,
                integrationException.Message,
                integrationException.Errors,
                correlationId));
            return;
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new ApiErrorResponse(
            "server_error",
            "An unexpected error occurred.",
            [],
            correlationId));
    });
});
app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapGet("/", () => Results.Ok(new
{
    service = "SSD.Api",
    description = "Special Sound & Screen Discovery monorepo API",
    version = "v1",
    environment = app.Environment.EnvironmentName
}));

app.MapGet("/api/moods", () => Results.Ok(Enum.GetNames<SSD.Domain.Enums.MoodCategory>()));

app.MapPost("/api/recommendations/discover", async (
    DiscoverRecommendationsRequest request,
    SSD.Application.Abstractions.IRecommendationService recommendationService,
    HttpContext httpContext,
    CancellationToken cancellationToken) =>
{
    var validationErrors = RecommendationRequestValidator.Validate(request);
    var correlationId = httpContext.TraceIdentifier;

    if (validationErrors.Count > 0)
    {
        return Results.BadRequest(new ApiErrorResponse(
            "validation_error",
            "The recommendation request was invalid.",
            validationErrors,
            correlationId));
    }

    var response = await recommendationService.DiscoverAsync(request, cancellationToken);
    return Results.Ok(response with { CorrelationId = correlationId });
});

var authGroup = app.MapGroup("/api/auth").RequireRateLimiting("auth");

authGroup.MapPost("/register", async (
    RegisterRequest request,
    IAuthService authService,
    HttpContext httpContext,
    CancellationToken cancellationToken) =>
{
    var errors = AuthValidators.Validate(request);
    if (errors.Count > 0)
    {
        return Results.BadRequest(new ApiErrorResponse(
            "validation_error",
            "The registration request was invalid.",
            errors,
            httpContext.TraceIdentifier));
    }

    var result = await authService.RegisterAsync(request, RequestContextFactory.Create(httpContext), cancellationToken);
    return Results.Created("/api/auth/me", result);
});

authGroup.MapPost("/login", async (
    LoginRequest request,
    IAuthService authService,
    HttpContext httpContext,
    CancellationToken cancellationToken) =>
{
    var errors = AuthValidators.Validate(request);
    if (errors.Count > 0)
    {
        return Results.BadRequest(new ApiErrorResponse(
            "validation_error",
            "The login request was invalid.",
            errors,
            httpContext.TraceIdentifier));
    }

    var result = await authService.LoginAsync(request, RequestContextFactory.Create(httpContext), cancellationToken);
    return Results.Ok(result);
});

authGroup.MapPost("/refresh", async (
    RefreshTokenRequest request,
    IAuthService authService,
    HttpContext httpContext,
    CancellationToken cancellationToken) =>
{
    var errors = AuthValidators.Validate(request);
    if (errors.Count > 0)
    {
        return Results.BadRequest(new ApiErrorResponse(
            "validation_error",
            "The refresh token request was invalid.",
            errors,
            httpContext.TraceIdentifier));
    }

    var result = await authService.RefreshAsync(request, RequestContextFactory.Create(httpContext), cancellationToken);
    return Results.Ok(result);
});

authGroup.MapPost("/logout", async (
    LogoutRequest request,
    IAuthService authService,
    HttpContext httpContext,
    CancellationToken cancellationToken) =>
{
    var errors = AuthValidators.Validate(request);
    if (errors.Count > 0)
    {
        return Results.BadRequest(new ApiErrorResponse(
            "validation_error",
            "The logout request was invalid.",
            errors,
            httpContext.TraceIdentifier));
    }

    await authService.LogoutAsync(request, RequestContextFactory.Create(httpContext), cancellationToken);
    return Results.NoContent();
});

app.MapGet("/api/auth/me", (ClaimsPrincipal user) =>
{
    if (user.Identity?.IsAuthenticated != true)
    {
        return Results.Unauthorized();
    }

    return Results.Ok(new
    {
        id = user.FindFirstValue(JwtRegisteredClaimNames.Sub),
        email = user.FindFirstValue(JwtRegisteredClaimNames.Email),
        displayName = user.FindFirstValue(ClaimTypes.Name),
        role = user.FindFirstValue(ClaimTypes.Role),
        sessionId = user.FindFirstValue("sid")
    });
}).RequireAuthorization();

var spotifyGroup = app.MapGroup("/api/spotify");

spotifyGroup.MapPost("/resolve-track", async (
    SpotifyResolveTrackRequest request,
    ISpotifyService spotifyService,
    HttpContext httpContext,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Url))
    {
        return Results.BadRequest(new ApiErrorResponse(
            "validation_error",
            "The Spotify track request was invalid.",
            ["Spotify track URL is required."],
            httpContext.TraceIdentifier));
    }

    var result = await spotifyService.ResolveTrackAsync(request.Url, cancellationToken);
    return Results.Ok(result);
});

spotifyGroup.MapGet("/link/start", async (
    ClaimsPrincipal user,
    ISpotifyService spotifyService,
    HttpContext httpContext,
    CancellationToken cancellationToken) =>
{
    var userId = user.GetUserId();
    if (userId is null)
    {
        return Results.Unauthorized();
    }

    var result = await spotifyService.CreateLinkStartAsync(userId.Value, cancellationToken);
    return Results.Ok(result);
}).RequireAuthorization();

spotifyGroup.MapGet("/link/callback", async (
    string code,
    string state,
    ISpotifyService spotifyService,
    CancellationToken cancellationToken) =>
{
    var result = await spotifyService.CompleteLinkAsync(code, state, cancellationToken);
    return Results.Ok(result);
});

spotifyGroup.MapGet("/me", async (
    ClaimsPrincipal user,
    ISpotifyService spotifyService,
    HttpContext httpContext,
    CancellationToken cancellationToken) =>
{
    var userId = user.GetUserId();
    if (userId is null)
    {
        return Results.Unauthorized();
    }

    var result = await spotifyService.GetLinkedAccountAsync(userId.Value, cancellationToken);
    return Results.Ok(result);
}).RequireAuthorization();

app.Run();

public partial class Program;
