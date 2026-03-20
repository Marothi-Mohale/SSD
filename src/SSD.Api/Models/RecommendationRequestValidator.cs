using SSD.Application.Contracts;

namespace SSD.Api.Models;

public static class RecommendationRequestValidator
{
    public static IReadOnlyList<string> Validate(DiscoverRecommendationsRequest request)
    {
        var errors = new List<string>();

        if (!request.IncludeMusic && !request.IncludeMovies)
        {
            errors.Add("Select at least one recommendation type.");
        }

        if (request.Energy is { Length: > 20 })
        {
            errors.Add("Energy must be 20 characters or fewer.");
        }

        if (request.TimeOfDay is { Length: > 20 })
        {
            errors.Add("TimeOfDay must be 20 characters or fewer.");
        }

        return errors;
    }
}
