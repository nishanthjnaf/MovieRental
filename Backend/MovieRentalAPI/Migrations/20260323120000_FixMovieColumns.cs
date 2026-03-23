using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieRentalAPI.Migrations
{
    /// <inheritdoc />
    public partial class FixMovieColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add Cast column if it doesn't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'Movies' AND COLUMN_NAME = 'Cast'
                )
                BEGIN
                    ALTER TABLE [Movies] ADD [Cast] nvarchar(max) NOT NULL DEFAULT '';
                END
            ");

            // Add Director column if it doesn't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'Movies' AND COLUMN_NAME = 'Director'
                )
                BEGIN
                    ALTER TABLE [Movies] ADD [Director] nvarchar(max) NOT NULL DEFAULT '';
                END
            ");

            // Add ContentRating column if it doesn't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'Movies' AND COLUMN_NAME = 'ContentRating'
                )
                BEGIN
                    ALTER TABLE [Movies] ADD [ContentRating] nvarchar(max) NOT NULL DEFAULT '';
                END
            ");

            // Add ContentAdvisory column if it doesn't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'Movies' AND COLUMN_NAME = 'ContentAdvisory'
                )
                BEGIN
                    ALTER TABLE [Movies] ADD [ContentAdvisory] nvarchar(max) NOT NULL DEFAULT '';
                END
            ");

            // Add PosterPath column if it doesn't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'Movies' AND COLUMN_NAME = 'PosterPath'
                )
                BEGIN
                    ALTER TABLE [Movies] ADD [PosterPath] nvarchar(max) NULL;
                END
            ");

            // Add TrailerUrl column if it doesn't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'Movies' AND COLUMN_NAME = 'TrailerUrl'
                )
                BEGIN
                    ALTER TABLE [Movies] ADD [TrailerUrl] nvarchar(max) NULL;
                END
            ");

            // Add RentalCount column if it doesn't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'Movies' AND COLUMN_NAME = 'RentalCount'
                )
                BEGIN
                    ALTER TABLE [Movies] ADD [RentalCount] int NOT NULL DEFAULT 0;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Cast", table: "Movies");
            migrationBuilder.DropColumn(name: "Director", table: "Movies");
            migrationBuilder.DropColumn(name: "ContentRating", table: "Movies");
            migrationBuilder.DropColumn(name: "ContentAdvisory", table: "Movies");
            migrationBuilder.DropColumn(name: "PosterPath", table: "Movies");
            migrationBuilder.DropColumn(name: "TrailerUrl", table: "Movies");
            migrationBuilder.DropColumn(name: "RentalCount", table: "Movies");
        }
    }
}
