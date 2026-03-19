using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieRentalAPI.Migrations
{
    /// <inheritdoc />
    public partial class MovieRentalAPI3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReturnDate",
                table: "RentalItems",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "DaysRented",
                table: "RentalItems",
                newName: "MovieId");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "RentalItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "RentalItems",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "RentalItems");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "RentalItems");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "RentalItems",
                newName: "ReturnDate");

            migrationBuilder.RenameColumn(
                name: "MovieId",
                table: "RentalItems",
                newName: "DaysRented");
        }
    }
}
