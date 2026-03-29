using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DuncanLaud.Infrastructure.Analytics.Migrations
{
    /// <inheritdoc />
    public partial class InitialAnalyticsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "analytics");

            migrationBuilder.CreateTable(
                name: "sessions",
                schema: "analytics",
                columns: table => new
                {
                    SessionId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IpHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    IpPrefix = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    UserAgentHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    UserAgentRaw = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    SessionStart = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    SessionEnd = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    EventCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsBot = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sessions", x => x.SessionId);
                });

            migrationBuilder.CreateTable(
                name: "api_events",
                schema: "analytics",
                columns: table => new
                {
                    ApiEventId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Endpoint = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    HttpMethod = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    StatusCode = table.Column<short>(type: "smallint", nullable: false),
                    LatencyMs = table.Column<int>(type: "int", nullable: true),
                    RequestBytes = table.Column<int>(type: "int", nullable: true),
                    ResponseBytes = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_api_events", x => x.ApiEventId);
                    table.ForeignKey(
                        name: "FK_api_events_sessions_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "analytics",
                        principalTable: "sessions",
                        principalColumn: "SessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "page_events",
                schema: "analytics",
                columns: table => new
                {
                    PageEventId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UrlPath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    UrlQuery = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Referrer = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    HttpMethod = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "GET"),
                    StatusCode = table.Column<short>(type: "smallint", nullable: false),
                    LatencyMs = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_page_events", x => x.PageEventId);
                    table.ForeignKey(
                        name: "FK_page_events_sessions_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "analytics",
                        principalTable: "sessions",
                        principalColumn: "SessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_api_events_endpoint",
                schema: "analytics",
                table: "api_events",
                columns: new[] { "Endpoint", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_api_events_session",
                schema: "analytics",
                table: "api_events",
                columns: new[] { "SessionId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_page_events_path",
                schema: "analytics",
                table: "page_events",
                columns: new[] { "UrlPath", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_page_events_session",
                schema: "analytics",
                table: "page_events",
                columns: new[] { "SessionId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_sessions_ip_hash",
                schema: "analytics",
                table: "sessions",
                columns: new[] { "IpHash", "SessionStart" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "api_events",
                schema: "analytics");

            migrationBuilder.DropTable(
                name: "page_events",
                schema: "analytics");

            migrationBuilder.DropTable(
                name: "sessions",
                schema: "analytics");
        }
    }
}
