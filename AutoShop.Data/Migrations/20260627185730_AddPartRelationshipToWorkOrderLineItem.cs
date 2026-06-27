using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoShop.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPartRelationshipToWorkOrderLineItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PartId",
                table: "WorkOrderLineItems",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderLineItems_PartId",
                table: "WorkOrderLineItems",
                column: "PartId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderLineItems_Parts_PartId",
                table: "WorkOrderLineItems",
                column: "PartId",
                principalTable: "Parts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderLineItems_Parts_PartId",
                table: "WorkOrderLineItems");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrderLineItems_PartId",
                table: "WorkOrderLineItems");

            migrationBuilder.DropColumn(
                name: "PartId",
                table: "WorkOrderLineItems");
        }
    }
}
