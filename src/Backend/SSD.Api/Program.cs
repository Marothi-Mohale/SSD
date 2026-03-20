using SSD.Api.Models;
using SSD.Application.Abstractions;
using SSD.Application.Contracts;
using SSD.Application.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();
builder.Services.AddSingleton<IRecommendationProvider, SeedRecommendationProvider>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.MapHealthChecks("/health");

app.MapGet("/", () => Results.Ok(new
{
    service = "SSD.Api",
    description = "Special Sound & Screen Discovery recommendation API",
    version = "v1"
}));

app.MapPost("/api/recommendations/discover", async (
    DiscoverRecommendationsRequest request,
    IRecommendationService recommendationService,
    HttpContext httpContext,
    CancellationToken cancellationToken) =>
{
    var validationErrors = RecommendationRequestValidator.Validate(request);
    var correlationId = httpContext.TraceIdentifier;

    if (validationErrors.Count > 0)
    {
        return Results.BadRequest(new ApiErrorResponse(
            "The recommendation request was invalid.",
            validationErrors,
            correlationId));
    }

    var response = await recommendationService.DiscoverAsync(request, cancellationToken);
    return Results.Ok(response with { CorrelationId = correlationId });
});

app.Run();

public partial class Program;
