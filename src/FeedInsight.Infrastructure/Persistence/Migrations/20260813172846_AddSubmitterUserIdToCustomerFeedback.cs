using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeedInsight.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSubmitterUserIdToCustomerFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SubmitterUserId",
                table: "CustomerFeedbacks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerFeedbacks_SubmitterUserId",
                table: "CustomerFeedbacks",
                column: "SubmitterUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerFeedbacks_Users_SubmitterUserId",
                table: "CustomerFeedbacks",
                column: "SubmitterUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerFeedbacks_Users_SubmitterUserId",
                table: "CustomerFeedbacks");

            migrationBuilder.DropIndex(
                name: "IX_CustomerFeedbacks_SubmitterUserId",
                table: "CustomerFeedbacks");

            migrationBuilder.DropColumn(
                name: "SubmitterUserId",
                table: "CustomerFeedbacks");
        }
    }
}
