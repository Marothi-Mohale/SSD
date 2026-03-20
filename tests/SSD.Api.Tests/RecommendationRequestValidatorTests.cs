using SSD.Api.Models;
using SSD.Application.Contracts;
using SSD.Domain.Enums;

namespace SSD.Api.Tests;

public sealed class RecommendationRequestValidatorTests
{
    [Fact]
    public void Validate_ReturnsError_WhenBothContentTypesAreDisabled()
    {
        var request = new DiscoverRecommendationsRequest(
            MoodCategory.Relaxed,
            "low",
            "evening",
            false,
            IncludeMusic: false,
            IncludeMovies: false);

        var errors = RecommendationRequestValidator.Validate(request);

        Assert.Contains("Select at least one recommendation type.", errors);
    }

    [Fact]
    public void Validate_ReturnsNoErrors_ForValidRequest()
    {
        var request = new DiscoverRecommendationsRequest(
            MoodCategory.RainyDay,
            "low",
            "night",
            true,
            IncludeMusic: true,
            IncludeMovies: true);

        var errors = RecommendationRequestValidator.Validate(request);

        Assert.Empty(errors);
    }
}
