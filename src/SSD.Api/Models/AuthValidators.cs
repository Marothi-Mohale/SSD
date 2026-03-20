using SSD.Application.Contracts.Auth;

namespace SSD.Api.Models;

public static class AuthValidators
{
    public static IReadOnlyList<string> Validate(RegisterRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors.Add("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            errors.Add("Display name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            errors.Add("Password is required.");
        }
        else
        {
            if (request.Password.Length < 12)
            {
                errors.Add("Password must be at least 12 characters.");
            }

            if (!request.Password.Any(char.IsUpper))
            {
                errors.Add("Password must include an uppercase letter.");
            }

            if (!request.Password.Any(char.IsLower))
            {
                errors.Add("Password must include a lowercase letter.");
            }

            if (!request.Password.Any(char.IsDigit))
            {
                errors.Add("Password must include a number.");
            }
        }

        return errors;
    }

    public static IReadOnlyList<string> Validate(LoginRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors.Add("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            errors.Add("Password is required.");
        }

        return errors;
    }

    public static IReadOnlyList<string> Validate(RefreshTokenRequest request)
    {
        return string.IsNullOrWhiteSpace(request.RefreshToken)
            ? ["Refresh token is required."]
            : [];
    }

    public static IReadOnlyList<string> Validate(LogoutRequest request)
    {
        return string.IsNullOrWhiteSpace(request.RefreshToken)
            ? ["Refresh token is required."]
            : [];
    }
}
