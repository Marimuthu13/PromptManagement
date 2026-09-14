using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartPrompt.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPromptTitleIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_prompts_title",
                table: "prompts",
                column: "title");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_prompts_title",
                table: "prompts");
        }
    }
}
