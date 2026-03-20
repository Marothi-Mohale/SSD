#pragma warning disable CA1848
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SSD.Application.Abstractions;
using SSD.Application.Contracts.Auth;
using SSD.Application.Exceptions;
using SSD.Domain.Entities;
using SSD.Domain.Enums;
using SSD.Domain.ValueObjects;
using SSD.Infrastructure.Persistence;

namespace SSD.Infrastructure.Auth;

public sealed class AuthService(
    SsdDbContext dbContext,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, AuthRequestContext context, CancellationToken cancellationToken = default)
    {
        var email = new EmailAddress(request.Email);
        var normalizedEmail = email.NormalizedValue;

        var existingUser = await dbContext.Users
            .AsNoTracking()
            .AnyAsync(user => user.Email.NormalizedValue == normalizedEmail, cancellationToken);

        if (existingUser)
        {
            throw new AuthException("email_in_use", "An account with that email already exists.");
        }

        var user = new User(
            Guid.NewGuid(),
            email,
            request.DisplayName.Trim(),
            passwordHasher.HashPassword(request.Password),
            "PBKDF2-SHA512");

        user.ConfirmEmail();
        user.RecordLogin();

        var nowUtc = DateTimeOffset.UtcNow;
        var refreshTokenResult = tokenService.CreateRefreshToken(
            user.Id,
            request.DeviceName,
            context.UserAgent,
            context.IpAddress,
            nowUtc);

        dbContext.Users.Add(user);
        dbContext.RefreshTokens.Add(refreshTokenResult.Token);

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Registered SSD user {UserId} with email {Email} from {IpAddress}",
            user.Id,
            user.Email.Value,
            context.IpAddress ?? "unknown");

        return CreateAuthResponse(user, refreshTokenResult, nowUtc);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, AuthRequestContext context, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = new EmailAddress(request.Email).NormalizedValue;

        var user = await dbContext.Users
            .SingleOrDefaultAsync(candidate => candidate.Email.NormalizedValue == normalizedEmail, cancellationToken);

        if (user is null || user.Status != UserStatus.Active || !passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            logger.LogWarning(
                "Rejected login attempt for email {Email} from {IpAddress}",
                request.Email.Trim(),
                context.IpAddress ?? "unknown");

            throw new AuthException("invalid_credentials", "The email or password is incorrect.");
        }

        var nowUtc = DateTimeOffset.UtcNow;
        user.RecordLogin(nowUtc);

        var refreshTokenResult = tokenService.CreateRefreshToken(
            user.Id,
            request.DeviceName,
            context.UserAgent,
            context.IpAddress,
            nowUtc);

        dbContext.RefreshTokens.Add(refreshTokenResult.Token);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Authenticated SSD user {UserId} with role {Role}",
            user.Id,
            user.Role);

        return CreateAuthResponse(user, refreshTokenResult, nowUtc);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshTokenRequest request, AuthRequestContext context, CancellationToken cancellationToken = default)
    {
        var tokenHash = tokenService.ComputeRefreshTokenHash(request.RefreshToken);
        var token = await dbContext.RefreshTokens
            .Include(candidate => candidate.User)
            .SingleOrDefaultAsync(candidate => candidate.TokenHash == tokenHash, cancellationToken);

        var nowUtc = DateTimeOffset.UtcNow;
        if (token?.User is null || token.User.Status != UserStatus.Active || !token.IsActive(nowUtc))
        {
            throw new AuthException("invalid_refresh_token", "The refresh token is invalid or expired.");
        }

        token.MarkUsed(nowUtc);

        var replacement = tokenService.CreateRefreshToken(
            token.UserId,
            request.DeviceName ?? token.DeviceName,
            context.UserAgent,
            context.IpAddress,
            nowUtc);

        token.Revoke(context.IpAddress, "Rotated", replacement.Token.TokenHash, nowUtc);
        dbContext.RefreshTokens.Add(replacement.Token);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Rotated refresh token for SSD user {UserId} and session {SessionId}",
            token.UserId,
            token.Id);

        return CreateAuthResponse(token.User, replacement, nowUtc);
    }

    public async Task LogoutAsync(LogoutRequest request, AuthRequestContext context, CancellationToken cancellationToken = default)
    {
        var tokenHash = tokenService.ComputeRefreshTokenHash(request.RefreshToken);
        var token = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(candidate => candidate.TokenHash == tokenHash, cancellationToken);

        var nowUtc = DateTimeOffset.UtcNow;
        if (token is null)
        {
            throw new AuthException("invalid_refresh_token", "The refresh token is invalid or expired.");
        }

        token.Revoke(context.IpAddress, request.LogoutFromAllDevices ? "Logout all devices" : "Logout", revokedUtc: nowUtc);

        if (request.LogoutFromAllDevices)
        {
            var activeTokens = await dbContext.RefreshTokens
                .Where(candidate => candidate.UserId == token.UserId && candidate.RevokedUtc == null && candidate.Id != token.Id)
                .ToListAsync(cancellationToken);

            foreach (var activeToken in activeTokens)
            {
                activeToken.Revoke(context.IpAddress, "Logout all devices", revokedUtc: nowUtc);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Logged out SSD user {UserId} from {Mode}",
            token.UserId,
            request.LogoutFromAllDevices ? "all devices" : "current device");
    }

    private AuthResponse CreateAuthResponse(User user, RefreshTokenResult refreshTokenResult, DateTimeOffset nowUtc)
    {
        var accessToken = tokenService.CreateAccessToken(user, refreshTokenResult.Token, nowUtc);

        return new AuthResponse(
            accessToken.Token,
            accessToken.ExpiresUtc,
            refreshTokenResult.PlainTextToken,
            refreshTokenResult.ExpiresUtc,
            new AuthUserResponse(
                user.Id,
                user.Email.Value,
                user.DisplayName,
                user.Role.ToString()));
    }
}
#pragma warning restore CA1848
