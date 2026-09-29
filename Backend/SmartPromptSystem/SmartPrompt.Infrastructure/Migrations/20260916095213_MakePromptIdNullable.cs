using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartPrompt.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakePromptIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_prompt_execution_logs_prompts_prompt_id",
                table: "PromptExecutionLogs");

            migrationBuilder.AlterColumn<Guid>(
                name: "prompt_id",
                table: "PromptExecutionLogs",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "fk_prompt_execution_logs_prompts_prompt_id",
                table: "PromptExecutionLogs",
                column: "prompt_id",
                principalTable: "prompts",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_prompt_execution_logs_prompts_prompt_id",
                table: "PromptExecutionLogs");

            migrationBuilder.AlterColumn<Guid>(
                name: "prompt_id",
                table: "PromptExecutionLogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_prompt_execution_logs_prompts_prompt_id",
                table: "PromptExecutionLogs",
                column: "prompt_id",
                principalTable: "prompts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
