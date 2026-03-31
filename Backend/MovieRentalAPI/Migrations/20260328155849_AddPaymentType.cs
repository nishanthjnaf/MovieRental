using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieRentalAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the unique constraint so one rental can have multiple payment records
            migrationBuilder.DropIndex(
                name: "IX_Payments_RentalId",
                table: "Payments");

            migrationBuilder.AddColumn<int>(
                name: "PaymentType",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0); // 0 = Purchase (all existing records are purchases)

            // Re-create as non-unique index
            migrationBuilder.CreateIndex(
                name: "IX_Payments_RentalId",
                table: "Payments",
                column: "RentalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_RentalId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaymentType",
                table: "Payments");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_RentalId",
                table: "Payments",
                column: "RentalId",
                unique: true);
        }
    }
}
