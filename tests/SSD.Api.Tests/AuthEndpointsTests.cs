using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using SSD.Application.Contracts.Auth;
using SSD.Api.Models;

namespace SSD.Api.Tests;

public sealed class AuthEndpointsTests(TestAuthWebApplicationFactory factory) : IClassFixture<TestAuthWebApplicationFactory>
{
    [Fact]
    public async Task Register_Login_Refresh_Logout_Flow_Works()
    {
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(
            "taylor@example.com",
            "StrongPass1234",
            "Taylor",
            "Pixel 9"));

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var registerPayload = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(registerPayload);
        Assert.False(string.IsNullOrWhiteSpace(registerPayload.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(registerPayload.RefreshToken));
        Assert.Equal("User", registerPayload.User.Role);

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            "taylor@example.com",
            "StrongPass1234",
            "Pixel 9"));

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var loginPayload = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(loginPayload);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginPayload.AccessToken);
        var meResponse = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        var refreshResponse = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(
            loginPayload.RefreshToken,
            "Pixel 9"));

        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
        var refreshPayload = await refreshResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(refreshPayload);
        Assert.NotEqual(loginPayload.RefreshToken, refreshPayload.RefreshToken);

        var reuseRefreshResponse = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(
            loginPayload.RefreshToken,
            "Pixel 9"));

        Assert.Equal(HttpStatusCode.Unauthorized, reuseRefreshResponse.StatusCode);

        var logoutResponse = await client.PostAsJsonAsync("/api/auth/logout", new LogoutRequest(
            refreshPayload.RefreshToken,
            false));

        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);

        var refreshAfterLogoutResponse = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(
            refreshPayload.RefreshToken,
            "Pixel 9"));

        Assert.Equal(HttpStatusCode.Unauthorized, refreshAfterLogoutResponse.StatusCode);
    }

    [Fact]
    public async Task Login_ReturnsConsistentError_ForInvalidPassword()
    {
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(
            "morgan@example.com",
            "AnotherStrong123",
            "Morgan",
            "iPhone"));

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            "morgan@example.com",
            "wrong-password",
            "iPhone"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(payload);
        Assert.Equal("invalid_credentials", payload.Code);
    }
}
