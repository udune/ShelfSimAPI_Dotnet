using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ShelfSimAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgreSQLMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Author = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ThicknessMn = table.Column<int>(type: "integer", nullable: false),
                    HeightMm = table.Column<int>(type: "integer", nullable: false),
                    Sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Runs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LayoutId = table.Column<int>(type: "integer", nullable: true),
                    RandomSeed = table.Column<int>(type: "integer", nullable: false),
                    HandleTimeSec = table.Column<float>(type: "real", nullable: false),
                    RobotSpeedCellsPerSec = table.Column<float>(type: "real", nullable: false),
                    TopN = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RunId = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CellCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    BookTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    StartTs = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndTs = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TravelTimeSec = table.Column<float>(type: "real", nullable: true),
                    HandleTimeSec = table.Column<float>(type: "real", nullable: true),
                    TotalTimeSec = table.Column<float>(type: "real", nullable: true),
                    PathLengthCells = table.Column<int>(type: "integer", nullable: true),
                    Result = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    FailReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RobotName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jobs_Runs_RunId",
                        column: x => x.RunId,
                        principalTable: "Runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Books_Title",
                table: "Books",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_RunId_CellCode",
                table: "Jobs",
                columns: new[] { "RunId", "CellCode" });

            migrationBuilder.CreateIndex(
                name: "IX_Runs_Status",
                table: "Runs",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Books");

            migrationBuilder.DropTable(
                name: "Jobs");

            migrationBuilder.DropTable(
                name: "Runs");
        }
    }
}
