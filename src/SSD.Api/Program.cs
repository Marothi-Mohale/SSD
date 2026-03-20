using SSD.Api.Models;
using SSD.Application.Contracts;
using SSD.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();
builder.Services.AddInfrastructure();

var app = builder.Build();

app.UseExceptionHandler();
app.UseHttpsRedirection();

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
            "The recommendation request was invalid.",
            validationErrors,
            correlationId));
    }

    var response = await recommendationService.DiscoverAsync(request, cancellationToken);
    return Results.Ok(response with { CorrelationId = correlationId });
});

app.Run();

public partial class Program;
