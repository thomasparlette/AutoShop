using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoShop.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkOrderMileageFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MileageIn",
                table: "WorkOrders",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MileageOut",
                table: "WorkOrders",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MileageIn",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "MileageOut",
                table: "WorkOrders");
        }
    }
}
