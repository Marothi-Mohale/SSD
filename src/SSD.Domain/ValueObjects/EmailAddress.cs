namespace SSD.Domain.ValueObjects;

public sealed record EmailAddress
{
    public EmailAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Email address is required.", nameof(value));
        }

        var trimmed = value.Trim();

        if (!trimmed.Contains('@', StringComparison.Ordinal))
        {
            throw new ArgumentException("Email address must contain '@'.", nameof(value));
        }

        Value = trimmed;
        NormalizedValue = trimmed.ToUpperInvariant();
    }

    private EmailAddress()
    {
        Value = string.Empty;
        NormalizedValue = string.Empty;
    }

    public string Value { get; private set; }

    public string NormalizedValue { get; private set; }

    public override string ToString() => Value;
}
