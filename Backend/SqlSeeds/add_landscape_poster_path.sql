-- Add LandscapePosterPath column to Movies table
-- Run this against your MovieRental database

IF COL_LENGTH('dbo.Movies', 'LandscapePosterPath') IS NULL
BEGIN
    ALTER TABLE dbo.Movies ADD LandscapePosterPath NVARCHAR(MAX) NULL;
    PRINT 'Column LandscapePosterPath added to Movies table.';
END
ELSE
BEGIN
    PRINT 'Column LandscapePosterPath already exists.';
END
