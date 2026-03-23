using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieRentalAPI.Migrations
{
    /// <inheritdoc />
    public partial class movierent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cast",
                table: "Movies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContentAdvisory",
                table: "Movies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContentRating",
                table: "Movies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Director",
                table: "Movies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_UserId",
                table: "Payments",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Users_UserId",
                table: "Payments",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Users_UserId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_UserId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Cast",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "ContentAdvisory",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "ContentRating",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "Director",
                table: "Movies");
        }
    }
}
