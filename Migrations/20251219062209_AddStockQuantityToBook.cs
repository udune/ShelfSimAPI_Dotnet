using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShelfSimAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddStockQuantityToBook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StockQuantity",
                table: "Books",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StockQuantity",
                table: "Books");
        }
    }
}
