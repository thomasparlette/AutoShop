using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoShop.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkOrderLineItemPartNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PartNumber",
                table: "WorkOrderLineItems",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PartNumber",
                table: "WorkOrderLineItems");
        }
    }
}
