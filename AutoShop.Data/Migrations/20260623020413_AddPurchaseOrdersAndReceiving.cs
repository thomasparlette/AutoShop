using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoShop.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseOrdersAndReceiving : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuantityReceived",
                table: "PurchaseOrderLineItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuantityReceived",
                table: "PurchaseOrderLineItems");
        }
    }
}
