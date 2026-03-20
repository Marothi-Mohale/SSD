using Microsoft.EntityFrameworkCore;
using SSD.Domain.Entities;

namespace SSD.Infrastructure.Persistence;

public sealed class SsdDbContext(DbContextOptions<SsdDbContext> options) : DbContext(options)
{
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<FavoriteItem> FavoriteItems => Set<FavoriteItem>();

    public DbSet<FeedbackEvent> FeedbackEvents => Set<FeedbackEvent>();

    public DbSet<LinkedSpotifyAccount> LinkedSpotifyAccounts => Set<LinkedSpotifyAccount>();

    public DbSet<MoodProfile> MoodProfiles => Set<MoodProfile>();

    public DbSet<RecommendationItem> RecommendationItems => Set<RecommendationItem>();

    public DbSet<RecommendationSession> RecommendationSessions => Set<RecommendationSession>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<SearchHistory> SearchHistoryEntries => Set<SearchHistory>();

    public DbSet<User> Users => Set<User>();

    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ssd");

        ConfigureUsers(modelBuilder);
        ConfigureUserPreferences(modelBuilder);
        ConfigureMoodProfiles(modelBuilder);
        ConfigureRefreshTokens(modelBuilder);
        ConfigureSpotifyAccounts(modelBuilder);
        ConfigureRecommendationSessions(modelBuilder);
        ConfigureRecommendationItems(modelBuilder);
        ConfigureFavorites(modelBuilder);
        ConfigureFeedback(modelBuilder);
        ConfigureSearchHistory(modelBuilder);
        ConfigureAuditLogs(modelBuilder);
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        var user = modelBuilder.Entity<User>();
        user.ToTable("users");
        user.HasKey(x => x.Id);
        user.Property(x => x.Id).ValueGeneratedNever();
        user.Property(x => x.DisplayName).HasMaxLength(120);
        user.Property(x => x.PasswordHash).HasMaxLength(512);
        user.Property(x => x.PasswordHashAlgorithm).HasMaxLength(64);
        user.Property(x => x.Role);
        user.Property(x => x.CreatedUtc).HasColumnType("timestamp with time zone");
        user.Property(x => x.UpdatedUtc).HasColumnType("timestamp with time zone");
        user.Property(x => x.EmailConfirmedUtc).HasColumnType("timestamp with time zone");
        user.Property(x => x.LastLoginUtc).HasColumnType("timestamp with time zone");

        user.OwnsOne(x => x.Email, email =>
        {
            email.Property(x => x.Value).HasColumnName("email").HasMaxLength(320);
            email.Property(x => x.NormalizedValue).HasColumnName("normalized_email").HasMaxLength(320);
        });

        user.HasIndex("normalized_email").IsUnique();
        user.HasMany(x => x.RefreshTokens).WithOne(x => x.User).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        user.HasMany(x => x.MoodProfiles).WithOne(x => x.User).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        user.HasMany(x => x.FavoriteItems).WithOne(x => x.User).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        user.HasMany(x => x.SearchHistoryEntries).WithOne(x => x.User).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureUserPreferences(ModelBuilder modelBuilder)
    {
        var preference = modelBuilder.Entity<UserPreference>();
        preference.ToTable("user_preferences");
        preference.HasKey(x => x.Id);
        preference.Property(x => x.Id).ValueGeneratedNever();
        preference.Property(x => x.CreatedUtc).HasColumnType("timestamp with time zone");
        preference.Property(x => x.UpdatedUtc).HasColumnType("timestamp with time zone");
        preference.Property(x => x.OnboardingCompletedUtc).HasColumnType("timestamp with time zone");
        preference.Property(x => x.PreferredLanguageCode).HasMaxLength(10);
        preference.Property(x => x.PreferredRegionCode).HasMaxLength(10);
        preference.HasIndex(x => x.UserId).IsUnique();
        preference.HasOne(x => x.User).WithOne(x => x.Preference).HasForeignKey<UserPreference>(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureMoodProfiles(ModelBuilder modelBuilder)
    {
        var profile = modelBuilder.Entity<MoodProfile>();
        profile.ToTable("mood_profiles");
        profile.HasKey(x => x.Id);
        profile.Property(x => x.Id).ValueGeneratedNever();
        profile.Property(x => x.CreatedUtc).HasColumnType("timestamp with time zone");
        profile.Property(x => x.UpdatedUtc).HasColumnType("timestamp with time zone");
        profile.Property(x => x.Name).HasMaxLength(80);
        profile.Property(x => x.Notes).HasColumnType("text");
        profile.Property(x => x.PreferredGenres).HasJsonbStringListConversion();
        profile.Property(x => x.AvoidedGenres).HasJsonbStringListConversion();
        profile.HasIndex(x => new { x.UserId, x.IsDefault }).HasFilter("\"is_default\" = true");
    }

    private static void ConfigureRefreshTokens(ModelBuilder modelBuilder)
    {
        var token = modelBuilder.Entity<RefreshToken>();
        token.ToTable("refresh_tokens");
        token.HasKey(x => x.Id);
        token.Property(x => x.Id).ValueGeneratedNever();
        token.Property(x => x.CreatedUtc).HasColumnType("timestamp with time zone");
        token.Property(x => x.UpdatedUtc).HasColumnType("timestamp with time zone");
        token.Property(x => x.ExpiresUtc).HasColumnType("timestamp with time zone");
        token.Property(x => x.RevokedUtc).HasColumnType("timestamp with time zone");
        token.Property(x => x.LastUsedUtc).HasColumnType("timestamp with time zone");
        token.Property(x => x.TokenHash).HasMaxLength(512);
        token.Property(x => x.DeviceName).HasMaxLength(120);
        token.Property(x => x.UserAgent).HasMaxLength(512);
        token.Property(x => x.CreatedByIp).HasMaxLength(64);
        token.Property(x => x.RevokedByIp).HasMaxLength(64);
        token.Property(x => x.ReplacedByTokenHash).HasMaxLength(512);
        token.Property(x => x.RevocationReason).HasMaxLength(256);
        token.HasIndex(x => x.TokenHash).IsUnique();
        token.HasIndex(x => new { x.UserId, x.ExpiresUtc });
    }

    private static void ConfigureSpotifyAccounts(ModelBuilder modelBuilder)
    {
        var account = modelBuilder.Entity<LinkedSpotifyAccount>();
        account.ToTable("linked_spotify_accounts");
        account.HasKey(x => x.Id);
        account.Property(x => x.Id).ValueGeneratedNever();
        account.Property(x => x.CreatedUtc).HasColumnType("timestamp with time zone");
        account.Property(x => x.UpdatedUtc).HasColumnType("timestamp with time zone");
        account.Property(x => x.LinkedUtc).HasColumnType("timestamp with time zone");
        account.Property(x => x.LastSyncedUtc).HasColumnType("timestamp with time zone");
        account.Property(x => x.RevokedUtc).HasColumnType("timestamp with time zone");
        account.Property(x => x.AccessTokenExpiresUtc).HasColumnType("timestamp with time zone");
        account.Property(x => x.SpotifyUserId).HasMaxLength(120);
        account.Property(x => x.SpotifyDisplayName).HasMaxLength(120);
        account.Property(x => x.CountryCode).HasMaxLength(5);
        account.Property(x => x.SubscriptionTier).HasMaxLength(50);
        account.Property(x => x.EncryptedRefreshToken).HasColumnType("text");
        account.Property(x => x.GrantedScopes).HasJsonbStringListConversion();
        account.HasIndex(x => x.UserId).IsUnique();
        account.HasIndex(x => x.SpotifyUserId).IsUnique();
        account.HasOne(x => x.User).WithOne(x => x.LinkedSpotifyAccount).HasForeignKey<LinkedSpotifyAccount>(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureRecommendationSessions(ModelBuilder modelBuilder)
    {
        var session = modelBuilder.Entity<RecommendationSession>();
        session.ToTable("recommendation_sessions");
        session.HasKey(x => x.Id);
        session.Property(x => x.Id).ValueGeneratedNever();
        session.Property(x => x.CreatedUtc).HasColumnType("timestamp with time zone");
        session.Property(x => x.UpdatedUtc).HasColumnType("timestamp with time zone");
        session.Property(x => x.RequestedUtc).HasColumnType("timestamp with time zone");
        session.Property(x => x.CompletedUtc).HasColumnType("timestamp with time zone");
        session.Property(x => x.CorrelationId).HasMaxLength(64);
        session.Property(x => x.FailureReason).HasMaxLength(512);
        session.HasIndex(x => x.CorrelationId).IsUnique();
        session.HasIndex(x => new { x.UserId, x.RequestedUtc });

        session.OwnsOne(x => x.Selection, selection =>
        {
            selection.Property(x => x.Mood).HasColumnName("selected_mood");
            selection.Property(x => x.EnergyLevel).HasColumnName("selected_energy_level");
            selection.Property(x => x.TimeOfDay).HasColumnName("selected_time_of_day");
            selection.Property(x => x.FamilyFriendlyOnly).HasColumnName("family_friendly_only");
            selection.Property(x => x.IncludeMusic).HasColumnName("include_music");
            selection.Property(x => x.IncludeMovies).HasColumnName("include_movies");
        });
    }

    private static void ConfigureRecommendationItems(ModelBuilder modelBuilder)
    {
        var item = modelBuilder.Entity<RecommendationItem>();
        item.ToTable("recommendation_items");
        item.HasKey(x => x.Id);
        item.Property(x => x.Id).ValueGeneratedNever();
        item.Property(x => x.CreatedUtc).HasColumnType("timestamp with time zone");
        item.Property(x => x.UpdatedUtc).HasColumnType("timestamp with time zone");
        item.Property(x => x.ProviderContentId).HasMaxLength(160);
        item.Property(x => x.ProviderContentUrl).HasMaxLength(500);
        item.Property(x => x.Title).HasMaxLength(250);
        item.Property(x => x.SecondaryText).HasMaxLength(250);
        item.Property(x => x.Description).HasColumnType("text");
        item.Property(x => x.Genres).HasJsonbStringListConversion();
        item.Property(x => x.MatchScore).HasPrecision(5, 4);
        item.Property(x => x.ArtworkUrl).HasMaxLength(500);
        item.Property(x => x.PreviewUrl).HasMaxLength(500);
        item.Property(x => x.ReleaseDate).HasColumnType("date");
        item.HasIndex(x => new { x.RecommendationSessionId, x.Rank }).IsUnique();
        item.HasIndex(x => new { x.Provider, x.ProviderContentId });

        item.OwnsOne(x => x.Explanation, explanation =>
        {
            explanation.Property(x => x.Summary).HasColumnName("explanation_summary").HasColumnType("text");
            explanation.Property(x => x.Signals).HasColumnName("explanation_signals").HasJsonbStringListConversion();
        });
    }

    private static void ConfigureFavorites(ModelBuilder modelBuilder)
    {
        var favorite = modelBuilder.Entity<FavoriteItem>();
        favorite.ToTable("favorite_items");
        favorite.HasKey(x => x.Id);
        favorite.Property(x => x.Id).ValueGeneratedNever();
        favorite.Property(x => x.CreatedUtc).HasColumnType("timestamp with time zone");
        favorite.Property(x => x.UpdatedUtc).HasColumnType("timestamp with time zone");
        favorite.Property(x => x.ProviderContentId).HasMaxLength(160);
        favorite.Property(x => x.ProviderContentUrl).HasMaxLength(500);
        favorite.Property(x => x.Title).HasMaxLength(250);
        favorite.Property(x => x.SecondaryText).HasMaxLength(250);
        favorite.Property(x => x.ArtworkUrl).HasMaxLength(500);
        favorite.HasIndex(x => new { x.UserId, x.ContentType, x.Provider, x.ProviderContentId }).IsUnique();
    }

    private static void ConfigureFeedback(ModelBuilder modelBuilder)
    {
        var feedback = modelBuilder.Entity<FeedbackEvent>();
        feedback.ToTable("feedback_events");
        feedback.HasKey(x => x.Id);
        feedback.Property(x => x.Id).ValueGeneratedNever();
        feedback.Property(x => x.CreatedUtc).HasColumnType("timestamp with time zone");
        feedback.Property(x => x.UpdatedUtc).HasColumnType("timestamp with time zone");
        feedback.Property(x => x.Reason).HasMaxLength(512);
        feedback.HasIndex(x => new { x.UserId, x.CreatedUtc });
        feedback.HasIndex(x => x.RecommendationSessionId);
        feedback.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        feedback.HasOne(x => x.RecommendationSession).WithMany(x => x.FeedbackEvents).HasForeignKey(x => x.RecommendationSessionId).OnDelete(DeleteBehavior.Cascade);
        feedback.HasOne(x => x.RecommendationItem).WithMany().HasForeignKey(x => x.RecommendationItemId).OnDelete(DeleteBehavior.SetNull);
    }

    private static void ConfigureSearchHistory(ModelBuilder modelBuilder)
    {
        var search = modelBuilder.Entity<SearchHistory>();
        search.ToTable("search_history");
        search.HasKey(x => x.Id);
        search.Property(x => x.Id).ValueGeneratedNever();
        search.Property(x => x.CreatedUtc).HasColumnType("timestamp with time zone");
        search.Property(x => x.UpdatedUtc).HasColumnType("timestamp with time zone");
        search.Property(x => x.QueryText).HasMaxLength(200);
        search.Property(x => x.FiltersJson).HasColumnType("jsonb");
        search.HasIndex(x => new { x.UserId, x.CreatedUtc });
        search.HasIndex(x => x.RecommendationSessionId);
        search.HasOne(x => x.RecommendationSession).WithMany().HasForeignKey(x => x.RecommendationSessionId).OnDelete(DeleteBehavior.SetNull);
    }

    private static void ConfigureAuditLogs(ModelBuilder modelBuilder)
    {
        var audit = modelBuilder.Entity<AuditLog>();
        audit.ToTable("audit_logs");
        audit.HasKey(x => x.Id);
        audit.Property(x => x.Id).ValueGeneratedNever();
        audit.Property(x => x.CreatedUtc).HasColumnType("timestamp with time zone");
        audit.Property(x => x.UpdatedUtc).HasColumnType("timestamp with time zone");
        audit.Property(x => x.Action).HasMaxLength(120);
        audit.Property(x => x.EntityName).HasMaxLength(120);
        audit.Property(x => x.EntityId).HasMaxLength(160);
        audit.Property(x => x.CorrelationId).HasMaxLength(64);
        audit.Property(x => x.IpAddress).HasMaxLength(64);
        audit.Property(x => x.MetadataJson).HasColumnType("jsonb");
        audit.HasIndex(x => x.CreatedUtc);
        audit.HasIndex(x => x.CorrelationId);
        audit.HasIndex(x => new { x.EntityName, x.EntityId });
        audit.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
    }
}
