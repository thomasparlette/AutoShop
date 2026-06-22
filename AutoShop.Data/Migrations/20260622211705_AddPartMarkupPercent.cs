using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoShop.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPartMarkupPercent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PartMarkupPercent",
                table: "ShopSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "ShopSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "PartMarkupPercent",
                value: 35m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PartMarkupPercent",
                table: "ShopSettings");
        }
    }
}
