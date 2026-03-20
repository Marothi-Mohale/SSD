namespace SSD.Domain.ValueObjects;

public sealed record RecommendationExplanation
{
    public RecommendationExplanation(string summary, IReadOnlyCollection<string>? signals = null)
    {
        if (string.IsNullOrWhiteSpace(summary))
        {
            throw new ArgumentException("Explanation summary is required.", nameof(summary));
        }

        Summary = summary.Trim();
        Signals = signals?.Where(signal => !string.IsNullOrWhiteSpace(signal))
            .Select(signal => signal.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList() ?? [];
    }

    private RecommendationExplanation()
    {
        Summary = string.Empty;
        Signals = [];
    }

    public string Summary { get; private set; }

    public List<string> Signals { get; private set; }
}
