using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShelfSimAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddConfigurationApi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CellsLayouts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    WarehouseX = table.Column<int>(type: "integer", nullable: false),
                    WarehouseY = table.Column<int>(type: "integer", nullable: false),
                    LayoutHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CellsLayouts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SimulationConfigs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    HandleTime = table.Column<float>(type: "real", nullable: false),
                    RobotSpeed = table.Column<float>(type: "real", nullable: false),
                    MoveTimeoutSec = table.Column<float>(type: "real", nullable: false),
                    TopN = table.Column<int>(type: "integer", nullable: false),
                    RandomSeed = table.Column<int>(type: "integer", nullable: false),
                    WarehousePosX = table.Column<int>(type: "integer", nullable: false),
                    WarehousePosY = table.Column<int>(type: "integer", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimulationConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CellDefs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    LayoutId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: false),
                    Height = table.Column<int>(type: "integer", nullable: false),
                    Orientation = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CellDefs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CellDefs_CellsLayouts_LayoutId",
                        column: x => x.LayoutId,
                        principalTable: "CellsLayouts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CellDefs_LayoutId_Code",
                table: "CellDefs",
                columns: new[] { "LayoutId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CellsLayouts_IsDefault",
                table: "CellsLayouts",
                column: "IsDefault",
                unique: true,
                filter: "\"IsDefault\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_SimulationConfigs_IsDefault",
                table: "SimulationConfigs",
                column: "IsDefault",
                unique: true,
                filter: "\"IsDefault\" = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CellDefs");

            migrationBuilder.DropTable(
                name: "SimulationConfigs");

            migrationBuilder.DropTable(
                name: "CellsLayouts");
        }
    }
}
