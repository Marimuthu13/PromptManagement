using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartPrompt.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAiCallMetadataToExecutionLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "display_title",
                table: "PromptExecutionLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "feature_type",
                table: "PromptExecutionLogs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "is_fallback",
                table: "PromptExecutionLogs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "latency_ms",
                table: "PromptExecutionLogs",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "model_used",
                table: "PromptExecutionLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "PromptExecutionLogs",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "display_title",
                table: "PromptExecutionLogs");

            migrationBuilder.DropColumn(
                name: "feature_type",
                table: "PromptExecutionLogs");

            migrationBuilder.DropColumn(
                name: "is_fallback",
                table: "PromptExecutionLogs");

            migrationBuilder.DropColumn(
                name: "latency_ms",
                table: "PromptExecutionLogs");

            migrationBuilder.DropColumn(
                name: "model_used",
                table: "PromptExecutionLogs");

            migrationBuilder.DropColumn(
                name: "status",
                table: "PromptExecutionLogs");
        }
    }
}
