using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeedInsight.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyAnalyticsSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyAnalyticsSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SnapshotDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TotalFeedbacksReceived = table.Column<int>(type: "int", nullable: false),
                    PositiveSentimentCount = table.Column<int>(type: "int", nullable: false),
                    NeutralSentimentCount = table.Column<int>(type: "int", nullable: false),
                    NegativeSentimentCount = table.Column<int>(type: "int", nullable: false),
                    TotalTasksExtracted = table.Column<int>(type: "int", nullable: false),
                    DraftTicketsGenerated = table.Column<int>(type: "int", nullable: false),
                    PoApprovalRatePercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    TopRequestedFeaturesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyAnalyticsSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyAnalyticsSnapshots_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyAnalyticsSnapshots_TenantId_SnapshotDate",
                table: "DailyAnalyticsSnapshots",
                columns: new[] { "TenantId", "SnapshotDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyAnalyticsSnapshots");
        }
    }
}
