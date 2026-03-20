using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSD.Infrastructure.Persistence.Migrations;

public partial class InitialDomainModel : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "ssd");

        migrationBuilder.CreateTable(
            name: "users",
            schema: "ssd",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                display_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                password_hash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                password_hash_algorithm = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                status = table.Column<int>(type: "integer", nullable: false),
                role = table.Column<int>(type: "integer", nullable: false),
                email_confirmed_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                last_login_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                created_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                normalized_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_users", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "audit_logs",
            schema: "ssd",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: true),
                actor_type = table.Column<int>(type: "integer", nullable: false),
                action = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                entity_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                entity_id = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                correlation_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                ip_address = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                metadata_json = table.Column<string>(type: "jsonb", nullable: false),
                created_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_audit_logs", x => x.id);
                table.ForeignKey(
                    name: "fk_audit_logs_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "ssd",
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "favorite_items",
            schema: "ssd",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                content_type = table.Column<int>(type: "integer", nullable: false),
                provider = table.Column<int>(type: "integer", nullable: false),
                provider_content_id = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                provider_content_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                title = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                secondary_text = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                artwork_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                source_recommendation_item_id = table.Column<Guid>(type: "uuid", nullable: true),
                created_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_favorite_items", x => x.id);
                table.ForeignKey(
                    name: "fk_favorite_items_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "ssd",
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "linked_spotify_accounts",
            schema: "ssd",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                spotify_user_id = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                spotify_display_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                country_code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                subscription_tier = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                encrypted_refresh_token = table.Column<string>(type: "text", nullable: false),
                encrypted_access_token = table.Column<string>(type: "text", nullable: true),
                access_token_expires_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                status = table.Column<int>(type: "integer", nullable: false),
                linked_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                last_synced_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                revoked_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                granted_scopes = table.Column<string>(type: "jsonb", nullable: false),
                created_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_linked_spotify_accounts", x => x.id);
                table.ForeignKey(
                    name: "fk_linked_spotify_accounts_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "ssd",
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "mood_profiles",
            schema: "ssd",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                mood = table.Column<int>(type: "integer", nullable: false),
                energy_level = table.Column<int>(type: "integer", nullable: true),
                time_of_day = table.Column<int>(type: "integer", nullable: true),
                family_friendly_only = table.Column<bool>(type: "boolean", nullable: false),
                include_music = table.Column<bool>(type: "boolean", nullable: false),
                include_movies = table.Column<bool>(type: "boolean", nullable: false),
                is_default = table.Column<bool>(type: "boolean", nullable: false),
                notes = table.Column<string>(type: "text", nullable: true),
                preferred_genres = table.Column<string>(type: "jsonb", nullable: false),
                avoided_genres = table.Column<string>(type: "jsonb", nullable: false),
                created_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_mood_profiles", x => x.id);
                table.ForeignKey(
                    name: "fk_mood_profiles_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "ssd",
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "recommendation_sessions",
            schema: "ssd",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                correlation_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                status = table.Column<int>(type: "integer", nullable: false),
                requested_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                completed_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                recommendation_count = table.Column<int>(type: "integer", nullable: false),
                failure_reason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                created_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                selected_mood = table.Column<int>(type: "integer", nullable: false),
                selected_energy_level = table.Column<int>(type: "integer", nullable: true),
                selected_time_of_day = table.Column<int>(type: "integer", nullable: true),
                family_friendly_only = table.Column<bool>(type: "boolean", nullable: false),
                include_music = table.Column<bool>(type: "boolean", nullable: false),
                include_movies = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_recommendation_sessions", x => x.id);
                table.ForeignKey(
                    name: "fk_recommendation_sessions_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "ssd",
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "refresh_tokens",
            schema: "ssd",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                token_hash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                expires_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                revoked_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                last_used_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                device_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                user_agent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                created_by_ip = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                revoked_by_ip = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                replaced_by_token_hash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                revocation_reason = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                created_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_refresh_tokens", x => x.id);
                table.ForeignKey(
                    name: "fk_refresh_tokens_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "ssd",
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "user_preferences",
            schema: "ssd",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                include_music_by_default = table.Column<bool>(type: "boolean", nullable: false),
                include_movies_by_default = table.Column<bool>(type: "boolean", nullable: false),
                family_friendly_only = table.Column<bool>(type: "boolean", nullable: false),
                allow_explicit_content = table.Column<bool>(type: "boolean", nullable: false),
                preferred_language_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                preferred_region_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                default_energy_level = table.Column<int>(type: "integer", nullable: true),
                default_time_of_day = table.Column<int>(type: "integer", nullable: true),
                onboarding_completed_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                created_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_preferences", x => x.id);
                table.ForeignKey(
                    name: "fk_user_preferences_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "ssd",
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "recommendation_items",
            schema: "ssd",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                recommendation_session_id = table.Column<Guid>(type: "uuid", nullable: false),
                content_type = table.Column<int>(type: "integer", nullable: false),
                provider = table.Column<int>(type: "integer", nullable: false),
                provider_content_id = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                provider_content_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                title = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                secondary_text = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                description = table.Column<string>(type: "text", nullable: true),
                genres = table.Column<string>(type: "jsonb", nullable: false),
                match_score = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                rank = table.Column<int>(type: "integer", nullable: false),
                is_family_friendly = table.Column<bool>(type: "boolean", nullable: false),
                release_date = table.Column<DateOnly>(type: "date", nullable: true),
                artwork_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                preview_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                created_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                explanation_summary = table.Column<string>(type: "text", nullable: false),
                explanation_signals = table.Column<string>(type: "jsonb", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_recommendation_items", x => x.id);
                table.ForeignKey(
                    name: "fk_recommendation_items_recommendation_sessions_recommendation_session_id",
                    column: x => x.recommendation_session_id,
                    principalSchema: "ssd",
                    principalTable: "recommendation_sessions",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "search_history",
            schema: "ssd",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                search_domain = table.Column<int>(type: "integer", nullable: false),
                query_text = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                filters_json = table.Column<string>(type: "jsonb", nullable: false),
                result_count = table.Column<int>(type: "integer", nullable: false),
                recommendation_session_id = table.Column<Guid>(type: "uuid", nullable: true),
                created_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_search_history", x => x.id);
                table.ForeignKey(
                    name: "fk_search_history_recommendation_sessions_recommendation_session_id",
                    column: x => x.recommendation_session_id,
                    principalSchema: "ssd",
                    principalTable: "recommendation_sessions",
                    principalColumn: "id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "fk_search_history_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "ssd",
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "feedback_events",
            schema: "ssd",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                recommendation_session_id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                recommendation_item_id = table.Column<Guid>(type: "uuid", nullable: true),
                feedback_type = table.Column<int>(type: "integer", nullable: false),
                reason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                created_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_feedback_events", x => x.id);
                table.ForeignKey(
                    name: "fk_feedback_events_recommendation_items_recommendation_item_id",
                    column: x => x.recommendation_item_id,
                    principalSchema: "ssd",
                    principalTable: "recommendation_items",
                    principalColumn: "id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "fk_feedback_events_recommendation_sessions_recommendation_session_id",
                    column: x => x.recommendation_session_id,
                    principalSchema: "ssd",
                    principalTable: "recommendation_sessions",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_feedback_events_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "ssd",
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "ix_audit_logs_correlation_id",
            schema: "ssd",
            table: "audit_logs",
            column: "correlation_id");

        migrationBuilder.CreateIndex(
            name: "ix_audit_logs_created_utc",
            schema: "ssd",
            table: "audit_logs",
            column: "created_utc");

        migrationBuilder.CreateIndex(
            name: "ix_audit_logs_entity_name_entity_id",
            schema: "ssd",
            table: "audit_logs",
            columns: new[] { "entity_name", "entity_id" });

        migrationBuilder.CreateIndex(
            name: "ix_audit_logs_user_id",
            schema: "ssd",
            table: "audit_logs",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "ix_favorite_items_user_id_content_type_provider_provider_content_id",
            schema: "ssd",
            table: "favorite_items",
            columns: new[] { "user_id", "content_type", "provider", "provider_content_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_feedback_events_recommendation_item_id",
            schema: "ssd",
            table: "feedback_events",
            column: "recommendation_item_id");

        migrationBuilder.CreateIndex(
            name: "ix_feedback_events_recommendation_session_id",
            schema: "ssd",
            table: "feedback_events",
            column: "recommendation_session_id");

        migrationBuilder.CreateIndex(
            name: "ix_feedback_events_user_id_created_utc",
            schema: "ssd",
            table: "feedback_events",
            columns: new[] { "user_id", "created_utc" });

        migrationBuilder.CreateIndex(
            name: "ix_linked_spotify_accounts_spotify_user_id",
            schema: "ssd",
            table: "linked_spotify_accounts",
            column: "spotify_user_id",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_linked_spotify_accounts_user_id",
            schema: "ssd",
            table: "linked_spotify_accounts",
            column: "user_id",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_mood_profiles_user_id_is_default",
            schema: "ssd",
            table: "mood_profiles",
            columns: new[] { "user_id", "is_default" },
            filter: "\"is_default\" = true");

        migrationBuilder.CreateIndex(
            name: "ix_recommendation_items_provider_provider_content_id",
            schema: "ssd",
            table: "recommendation_items",
            columns: new[] { "provider", "provider_content_id" });

        migrationBuilder.CreateIndex(
            name: "ix_recommendation_items_recommendation_session_id_rank",
            schema: "ssd",
            table: "recommendation_items",
            columns: new[] { "recommendation_session_id", "rank" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_recommendation_sessions_correlation_id",
            schema: "ssd",
            table: "recommendation_sessions",
            column: "correlation_id",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_recommendation_sessions_user_id_requested_utc",
            schema: "ssd",
            table: "recommendation_sessions",
            columns: new[] { "user_id", "requested_utc" });

        migrationBuilder.CreateIndex(
            name: "ix_refresh_tokens_token_hash",
            schema: "ssd",
            table: "refresh_tokens",
            column: "token_hash",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_refresh_tokens_user_id_expires_utc",
            schema: "ssd",
            table: "refresh_tokens",
            columns: new[] { "user_id", "expires_utc" });

        migrationBuilder.CreateIndex(
            name: "ix_search_history_recommendation_session_id",
            schema: "ssd",
            table: "search_history",
            column: "recommendation_session_id");

        migrationBuilder.CreateIndex(
            name: "ix_search_history_user_id_created_utc",
            schema: "ssd",
            table: "search_history",
            columns: new[] { "user_id", "created_utc" });

        migrationBuilder.CreateIndex(
            name: "ix_user_preferences_user_id",
            schema: "ssd",
            table: "user_preferences",
            column: "user_id",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_users_normalized_email",
            schema: "ssd",
            table: "users",
            column: "normalized_email",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "audit_logs", schema: "ssd");
        migrationBuilder.DropTable(name: "favorite_items", schema: "ssd");
        migrationBuilder.DropTable(name: "feedback_events", schema: "ssd");
        migrationBuilder.DropTable(name: "linked_spotify_accounts", schema: "ssd");
        migrationBuilder.DropTable(name: "mood_profiles", schema: "ssd");
        migrationBuilder.DropTable(name: "refresh_tokens", schema: "ssd");
        migrationBuilder.DropTable(name: "search_history", schema: "ssd");
        migrationBuilder.DropTable(name: "user_preferences", schema: "ssd");
        migrationBuilder.DropTable(name: "recommendation_items", schema: "ssd");
        migrationBuilder.DropTable(name: "recommendation_sessions", schema: "ssd");
        migrationBuilder.DropTable(name: "users", schema: "ssd");
    }
}
