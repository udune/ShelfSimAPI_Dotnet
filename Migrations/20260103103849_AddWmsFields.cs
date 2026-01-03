using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShelfSimAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddWmsFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "SnapshotHumid",
                table: "Jobs",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SnapshotLightLeak",
                table: "Jobs",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "SnapshotTemp",
                table: "Jobs",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkerId",
                table: "Jobs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryDate",
                table: "Books",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Books",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EnvironmentConfigs",
                columns: table => new
                {
                    ConfigKey = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TargetValue = table.Column<float>(type: "real", nullable: false),
                    Tolerance = table.Column<float>(type: "real", nullable: false),
                    BoolValue = table.Column<bool>(type: "boolean", nullable: true),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnvironmentConfigs", x => x.ConfigKey);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EnvironmentConfigs");

            migrationBuilder.DropColumn(
                name: "SnapshotHumid",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "SnapshotLightLeak",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "SnapshotTemp",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "WorkerId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Books");
        }
    }
}
