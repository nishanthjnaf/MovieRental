SET NOCOUNT ON;
BEGIN TRANSACTION;

-- Ensure sensible defaults for existing schema
IF NOT EXISTS (
    SELECT 1
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
    INNER JOIN sys.tables t ON t.object_id = c.object_id
    WHERE t.name = 'Movies' AND c.name = 'Rating'
)
BEGIN
    ALTER TABLE dbo.Movies ADD CONSTRAINT DF_Movies_Rating DEFAULT (0) FOR Rating;
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
    INNER JOIN sys.tables t ON t.object_id = c.object_id
    WHERE t.name = 'Movies' AND c.name = 'RentalCount'
)
BEGIN
    ALTER TABLE dbo.Movies ADD CONSTRAINT DF_Movies_RentalCount DEFAULT (0) FOR RentalCount;
END;

UPDATE dbo.Movies SET Rating = 0 WHERE Rating IS NULL;
UPDATE dbo.Movies SET RentalCount = 0 WHERE RentalCount IS NULL;

-- 1) Seed genres first
DECLARE @GenreSeed TABLE (
    Name NVARCHAR(80),
    [Description] NVARCHAR(200)
);

INSERT INTO @GenreSeed (Name, [Description]) VALUES
('Action', 'High-energy stories with combat and stunts'),
('Adventure', 'Journeys, quests, and exploration'),
('Animation', 'Animated films for all audiences'),
('Comedy', 'Humor-driven storytelling'),
('Crime', 'Criminal investigations and underworld stories'),
('Drama', 'Character-focused emotional narratives'),
('Fantasy', 'Magical worlds and mythic elements'),
('Horror', 'Fear, tension, and suspense'),
('Mystery', 'Investigation and puzzle-driven plots'),
('Romance', 'Love-centered storytelling'),
('Sci-Fi', 'Futuristic and speculative science themes'),
('Thriller', 'Suspenseful high-stakes storytelling');

INSERT INTO dbo.Genres (Name, [Description])
SELECT gs.Name, gs.[Description]
FROM @GenreSeed gs
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Genres g WHERE g.Name = gs.Name
);

-- 2) Build 100 movies with varied prices and rotating genres
DECLARE @MovieSeed TABLE (
    Title NVARCHAR(200),
    [Description] NVARCHAR(600),
    ReleaseYear INT,
    DurationMinutes INT,
    [Language] NVARCHAR(40),
    CastCsv NVARCHAR(500),
    Director NVARCHAR(120),
    ContentRating NVARCHAR(20),
    ContentAdvisory NVARCHAR(200),
    GenreName NVARCHAR(80),
    RentalPrice REAL
);

;WITH N AS (
    SELECT TOP (100) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n
    FROM sys.all_objects
)
INSERT INTO @MovieSeed (
    Title, [Description], ReleaseYear, DurationMinutes, [Language], CastCsv,
    Director, ContentRating, ContentAdvisory, GenreName, RentalPrice
)
SELECT
    CONCAT('Seed Movie ', RIGHT(CONCAT('000', n), 3)) AS Title,
    CONCAT('Auto-seeded catalog movie #', n, ' for testing rentals, transactions, and watchlist flows.') AS [Description],
    1998 + (n % 28) AS ReleaseYear,
    92 + (n % 65) AS DurationMinutes,
    CASE (n % 5)
        WHEN 0 THEN 'English'
        WHEN 1 THEN 'Hindi'
        WHEN 2 THEN 'Tamil'
        WHEN 3 THEN 'Telugu'
        ELSE 'Korean'
    END AS [Language],
    CONCAT('Actor ', n, 'A, Actor ', n, 'B') AS CastCsv,
    CONCAT('Director ', n) AS Director,
    CASE (n % 4)
        WHEN 0 THEN 'PG'
        WHEN 1 THEN 'PG-13'
        WHEN 2 THEN 'U/A 16+'
        ELSE 'R'
    END AS ContentRating,
    CASE (n % 6)
        WHEN 0 THEN 'Action Violence'
        WHEN 1 THEN 'Language'
        WHEN 2 THEN 'Thematic Elements'
        WHEN 3 THEN 'Mild Peril'
        WHEN 4 THEN 'Fantasy Violence'
        ELSE 'Intense Scenes'
    END AS ContentAdvisory,
    CASE (n % 12)
        WHEN 0 THEN 'Action'
        WHEN 1 THEN 'Adventure'
        WHEN 2 THEN 'Animation'
        WHEN 3 THEN 'Comedy'
        WHEN 4 THEN 'Crime'
        WHEN 5 THEN 'Drama'
        WHEN 6 THEN 'Fantasy'
        WHEN 7 THEN 'Horror'
        WHEN 8 THEN 'Mystery'
        WHEN 9 THEN 'Romance'
        WHEN 10 THEN 'Sci-Fi'
        ELSE 'Thriller'
    END AS GenreName,
    CAST(89 + (n % 17) * 6 + (n % 5) * 2 AS REAL) AS RentalPrice
FROM N;

INSERT INTO dbo.Movies
(
    Title, [Description], ReleaseYear, DurationMinutes, [Language], Rating, Cast, Director,
    ContentRating, ContentAdvisory, RentalCount, MyProperty, PosterPath, TrailerUrl
)
SELECT
    ms.Title,
    ms.[Description],
    ms.ReleaseYear,
    ms.DurationMinutes,
    ms.[Language],
    0,
    ms.CastCsv,
    ms.Director,
    ms.ContentRating,
    ms.ContentAdvisory,
    0,
    0,
    CONCAT('https://picsum.photos/seed/', LOWER(REPLACE(ms.Title, ' ', '-')), '/400/240'),
    CONCAT('https://www.youtube.com/results?search_query=', REPLACE(ms.Title, ' ', '+'), '+official+trailer')
FROM @MovieSeed ms
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Movies m WHERE m.Title = ms.Title
);

-- 3) Map each movie to one genre
INSERT INTO dbo.GenreMovie (GenresId, MoviesId)
SELECT g.Id, m.Id
FROM @MovieSeed ms
INNER JOIN dbo.Genres g ON g.Name = ms.GenreName
INNER JOIN dbo.Movies m ON m.Title = ms.Title
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.GenreMovie gm
    WHERE gm.GenresId = g.Id AND gm.MoviesId = m.Id
);

-- 4) Add one inventory row per movie with varied rental prices
INSERT INTO dbo.Inventories (MovieId, RentalPrice, IsAvailable)
SELECT m.Id, ms.RentalPrice, 1
FROM @MovieSeed ms
INNER JOIN dbo.Movies m ON m.Title = ms.Title
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Inventories i WHERE i.MovieId = m.Id
);

COMMIT TRANSACTION;
PRINT 'Seed complete: genres, 100 movies, mappings, and inventories with varied prices.';
