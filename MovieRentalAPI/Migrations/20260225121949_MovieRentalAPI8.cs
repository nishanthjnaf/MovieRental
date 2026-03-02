using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieRentalAPI.Migrations
{
    /// <inheritdoc />
    public partial class MovieRentalAPI8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RentalItems_Inventories_InventoryId",
                table: "RentalItems");

            migrationBuilder.AlterColumn<int>(
                name: "InventoryId",
                table: "RentalItems",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_RentalItems_Inventories_InventoryId",
                table: "RentalItems",
                column: "InventoryId",
                principalTable: "Inventories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RentalItems_Inventories_InventoryId",
                table: "RentalItems");

            migrationBuilder.AlterColumn<int>(
                name: "InventoryId",
                table: "RentalItems",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RentalItems_Inventories_InventoryId",
                table: "RentalItems",
                column: "InventoryId",
                principalTable: "Inventories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
