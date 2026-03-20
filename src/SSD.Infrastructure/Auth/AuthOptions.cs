namespace SSD.Infrastructure.Auth;

public sealed class AuthOptions
{
    public const string SectionName = "Security";

    public string JwtIssuer { get; set; } = "SSD.Api";

    public string JwtAudience { get; set; } = "SSD.Mobile";

    public string JwtSigningKey { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 15;

    public int RefreshTokenDays { get; set; } = 30;
}
