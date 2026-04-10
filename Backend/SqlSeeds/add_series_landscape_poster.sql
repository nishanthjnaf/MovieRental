-- Add LandscapePosterPath column to Series table
IF COL_LENGTH('dbo.Series', 'LandscapePosterPath') IS NULL
BEGIN
    ALTER TABLE dbo.Series ADD LandscapePosterPath NVARCHAR(MAX) NULL;
    PRINT 'Column LandscapePosterPath added to Series table.';
END
ELSE
    PRINT 'Column LandscapePosterPath already exists.';
