using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShelfSimAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddLayoutAndUpdatedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "LayoutId",
                table: "Runs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<float>(
                name: "MoveTimeoutSec",
                table: "Runs",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "ErrorCode",
                table: "Jobs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Layouts",
                columns: table => new
                {
                    LayoutId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SchemaVersion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    GridSizeX = table.Column<int>(type: "integer", nullable: false),
                    GridSizeY = table.Column<int>(type: "integer", nullable: false),
                    WarehouseX = table.Column<int>(type: "integer", nullable: false),
                    WarehouseY = table.Column<int>(type: "integer", nullable: false),
                    CellsJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CellCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Layouts", x => x.LayoutId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Runs_LayoutId",
                table: "Runs",
                column: "LayoutId");

            migrationBuilder.CreateIndex(
                name: "IX_Layouts_CreatedAt",
                table: "Layouts",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Layouts");

            migrationBuilder.DropIndex(
                name: "IX_Runs_LayoutId",
                table: "Runs");

            migrationBuilder.DropColumn(
                name: "MoveTimeoutSec",
                table: "Runs");

            migrationBuilder.DropColumn(
                name: "ErrorCode",
                table: "Jobs");

            migrationBuilder.AlterColumn<int>(
                name: "LayoutId",
                table: "Runs",
                type: "integer",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
