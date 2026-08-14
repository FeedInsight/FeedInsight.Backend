using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeedInsight.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addfeedbackcomments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FeedbackComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerFeedbackId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeedbackComments_CustomerFeedbacks_CustomerFeedbackId",
                        column: x => x.CustomerFeedbackId,
                        principalTable: "CustomerFeedbacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeedbackComments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackComments_CustomerFeedbackId",
                table: "FeedbackComments",
                column: "CustomerFeedbackId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackComments_UserId",
                table: "FeedbackComments",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeedbackComments");
        }
    }
}
