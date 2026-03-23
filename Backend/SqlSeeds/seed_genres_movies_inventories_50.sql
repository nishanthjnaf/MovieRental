SET NOCOUNT ON;
BEGIN TRANSACTION;

-- 1) Ensure default values for movies (new rows default to 0)
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

-- Optional data correction for existing rows
UPDATE dbo.Movies
SET Rating = 0
WHERE Rating IS NULL;

UPDATE dbo.Movies
SET RentalCount = 0
WHERE RentalCount IS NULL;

-- 2) Seed Genres
DECLARE @GenreSeed TABLE (
    Name NVARCHAR(80),
    [Description] NVARCHAR(200)
);

INSERT INTO @GenreSeed (Name, [Description]) VALUES
('Action', 'High-energy films with stunts and combat'),
('Adventure', 'Exploration, quests, and epic journeys'),
('Animation', 'Animated storytelling for all ages'),
('Comedy', 'Lighthearted and humorous films'),
('Crime', 'Stories centered on criminal acts and justice'),
('Drama', 'Character-driven emotional narratives'),
('Fantasy', 'Magic, mythical worlds, and supernatural elements'),
('Mystery', 'Investigative and puzzle-driven stories'),
('Sci-Fi', 'Science and futuristic speculative storytelling'),
('Thriller', 'Suspense-driven and tense narratives');

INSERT INTO dbo.Genres (Name, [Description])
SELECT gs.Name, gs.[Description]
FROM @GenreSeed gs
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Genres g WHERE g.Name = gs.Name
);

-- 3) Seed 50 Movies
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

INSERT INTO @MovieSeed
(Title, [Description], ReleaseYear, DurationMinutes, [Language], CastCsv, Director, ContentRating, ContentAdvisory, GenreName, RentalPrice)
VALUES
('Dune Part Two', 'Paul Atreides unites with the Fremen for revenge and destiny.', 2024, 166, 'English', 'Timothee Chalamet, Zendaya', 'Denis Villeneuve', 'PG-13', 'Violence, Intense Scenes', 'Sci-Fi', 139),
('Oppenheimer', 'The story of J. Robert Oppenheimer and the atomic bomb project.', 2023, 180, 'English', 'Cillian Murphy, Emily Blunt', 'Christopher Nolan', 'R', 'Language, Disturbing Content', 'Drama', 149),
('Barbie', 'Barbie journeys from Barbie Land into the real world.', 2023, 114, 'English', 'Margot Robbie, Ryan Gosling', 'Greta Gerwig', 'PG-13', 'Suggestive Humor', 'Comedy', 119),
('Avatar The Way of Water', 'Jake Sully protects his family on Pandora amid new threats.', 2022, 192, 'English', 'Sam Worthington, Zoe Saldana', 'James Cameron', 'PG-13', 'Action, Intense Sequences', 'Adventure', 149),
('Top Gun Maverick', 'Maverick trains a new generation of elite fighter pilots.', 2022, 131, 'English', 'Tom Cruise, Miles Teller', 'Joseph Kosinski', 'PG-13', 'Action, Language', 'Action', 129),
('The Batman', 'Batman uncovers corruption while hunting the Riddler in Gotham.', 2022, 176, 'English', 'Robert Pattinson, Zoe Kravitz', 'Matt Reeves', 'PG-13', 'Violence, Dark Themes', 'Crime', 129),
('Spider Man No Way Home', 'Peter Parker faces multiverse villains after his identity is revealed.', 2021, 148, 'English', 'Tom Holland, Zendaya', 'Jon Watts', 'PG-13', 'Action, Mild Language', 'Action', 129),
('Doctor Strange in the Multiverse of Madness', 'Doctor Strange navigates dangerous multiverse realities.', 2022, 126, 'English', 'Benedict Cumberbatch, Elizabeth Olsen', 'Sam Raimi', 'PG-13', 'Fantasy Violence', 'Fantasy', 119),
('Black Panther Wakanda Forever', 'Wakanda confronts grief and a powerful new undersea kingdom.', 2022, 161, 'English', 'Letitia Wright, Angela Bassett', 'Ryan Coogler', 'PG-13', 'Action Violence', 'Action', 129),
('Mission Impossible Dead Reckoning', 'Ethan Hunt races to stop a global AI threat.', 2023, 163, 'English', 'Tom Cruise, Hayley Atwell', 'Christopher McQuarrie', 'PG-13', 'Intense Action', 'Action', 139),
('John Wick Chapter 4', 'John Wick takes on the High Table in a final showdown.', 2023, 169, 'English', 'Keanu Reeves, Donnie Yen', 'Chad Stahelski', 'R', 'Strong Violence', 'Action', 139),
('The Killer', 'A meticulous assassin spirals after a high-stakes miss.', 2023, 118, 'English', 'Michael Fassbender, Tilda Swinton', 'David Fincher', 'R', 'Violence, Language', 'Thriller', 119),
('Extraction 2', 'A black-ops mercenary returns for another impossible rescue.', 2023, 123, 'English', 'Chris Hemsworth, Golshifteh Farahani', 'Sam Hargrave', 'R', 'Strong Violence', 'Action', 119),
('The Gray Man', 'A CIA operative is hunted across continents by a rogue mercenary.', 2022, 129, 'English', 'Ryan Gosling, Chris Evans', 'Russo Brothers', 'PG-13', 'Violence, Language', 'Thriller', 119),
('The Hunger Games The Ballad of Songbirds and Snakes', 'A young Coriolanus Snow mentors a tribute before his rise to power.', 2023, 157, 'English', 'Tom Blyth, Rachel Zegler', 'Francis Lawrence', 'PG-13', 'Violence, Thematic Elements', 'Adventure', 129),
('Wonka', 'Young Willy Wonka dreams of opening a magical chocolate shop.', 2023, 116, 'English', 'Timothee Chalamet, Olivia Colman', 'Paul King', 'PG', 'Mild Peril', 'Fantasy', 109),
('Inside Out 2', 'Rileys emotions face new challenges during her teenage years.', 2024, 96, 'English', 'Amy Poehler, Maya Hawke', 'Kelsey Mann', 'PG', 'Mild Thematic Elements', 'Animation', 109),
('Kung Fu Panda 4', 'Po must train a successor while facing a shape-shifting villain.', 2024, 94, 'English', 'Jack Black, Awkwafina', 'Mike Mitchell', 'PG', 'Cartoon Action', 'Animation', 109),
('Moana', 'A courageous teen sails to restore the heart of Te Fiti.', 2016, 107, 'English', 'Aulii Cravalho, Dwayne Johnson', 'Ron Clements', 'PG', 'Mild Peril', 'Animation', 99),
('Frozen II', 'Elsa and Anna journey to discover the source of Elsas powers.', 2019, 103, 'English', 'Idina Menzel, Kristen Bell', 'Chris Buck', 'PG', 'Fantasy Peril', 'Animation', 99),
('Encanto', 'A magical family in Colombia discovers the strength of imperfections.', 2021, 102, 'English', 'Stephanie Beatriz, John Leguizamo', 'Jared Bush', 'PG', 'Mild Peril', 'Animation', 99),
('Coco', 'A boy enters the Land of the Dead to uncover family history.', 2017, 105, 'English', 'Anthony Gonzalez, Gael Garcia Bernal', 'Lee Unkrich', 'PG', 'Thematic Elements', 'Animation', 99),
('Soul', 'A jazz musician explores purpose between life and the afterlife.', 2020, 100, 'English', 'Jamie Foxx, Tina Fey', 'Pete Docter', 'PG', 'Mild Themes', 'Animation', 99),
('Luca', 'Two sea monsters enjoy a life-changing summer on the Italian Riviera.', 2021, 95, 'English', 'Jacob Tremblay, Jack Dylan Grazer', 'Enrico Casarosa', 'PG', 'Mild Peril', 'Animation', 99),
('Elemental', 'Fire and water discover connection in Element City.', 2023, 101, 'English', 'Leah Lewis, Mamoudou Athie', 'Peter Sohn', 'PG', 'Mild Thematic Elements', 'Animation', 99),
('Interstellar', 'Explorers travel through a wormhole to save humanity.', 2014, 169, 'English', 'Matthew McConaughey, Anne Hathaway', 'Christopher Nolan', 'PG-13', 'Intense Scenes', 'Sci-Fi', 129),
('Inception', 'A thief enters dreams to implant an idea into a targets mind.', 2010, 148, 'English', 'Leonardo DiCaprio, Joseph Gordon-Levitt', 'Christopher Nolan', 'PG-13', 'Violence, Language', 'Sci-Fi', 119),
('The Dark Knight', 'Batman battles the Joker as Gotham descends into chaos.', 2008, 152, 'English', 'Christian Bale, Heath Ledger', 'Christopher Nolan', 'PG-13', 'Violence, Intense Sequences', 'Crime', 119),
('Mad Max Fury Road', 'In a wasteland, rebels flee a tyrant in a relentless chase.', 2015, 120, 'English', 'Tom Hardy, Charlize Theron', 'George Miller', 'R', 'Strong Violence', 'Action', 119),
('Blade Runner 2049', 'A new blade runner uncovers a secret that could destabilize society.', 2017, 164, 'English', 'Ryan Gosling, Harrison Ford', 'Denis Villeneuve', 'R', 'Violence, Language', 'Sci-Fi', 129),
('The Matrix', 'A hacker discovers reality is a simulated world.', 1999, 136, 'English', 'Keanu Reeves, Laurence Fishburne', 'The Wachowskis', 'R', 'Sci-Fi Violence', 'Sci-Fi', 109),
('The Shawshank Redemption', 'Two imprisoned men bond over hope and resilience.', 1994, 142, 'English', 'Tim Robbins, Morgan Freeman', 'Frank Darabont', 'R', 'Mature Themes', 'Drama', 109),
('Forrest Gump', 'A kind-hearted man witnesses defining moments of American history.', 1994, 142, 'English', 'Tom Hanks, Robin Wright', 'Robert Zemeckis', 'PG-13', 'Thematic Elements', 'Drama', 109),
('Fight Club', 'An office worker forms an underground club with chaotic consequences.', 1999, 139, 'English', 'Brad Pitt, Edward Norton', 'David Fincher', 'R', 'Violence, Language', 'Drama', 109),
('The Social Network', 'The rise of Facebook and the legal battles that followed.', 2010, 120, 'English', 'Jesse Eisenberg, Andrew Garfield', 'David Fincher', 'PG-13', 'Language', 'Drama', 109),
('Whiplash', 'A young drummer pushes beyond limits under a ruthless mentor.', 2014, 106, 'English', 'Miles Teller, J.K. Simmons', 'Damien Chazelle', 'R', 'Language, Intensity', 'Drama', 109),
('Parasite', 'A poor family infiltrates a wealthy household with dark consequences.', 2019, 132, 'Korean', 'Song Kang-ho, Choi Woo-shik', 'Bong Joon-ho', 'R', 'Violence, Language', 'Thriller', 119),
('Everything Everywhere All at Once', 'A laundromat owner is pulled into a bizarre multiverse conflict.', 2022, 139, 'English', 'Michelle Yeoh, Ke Huy Quan', 'Dan Kwan and Daniel Scheinert', 'R', 'Violence, Language', 'Sci-Fi', 119),
('La La Land', 'A pianist and actress chase dreams in Los Angeles.', 2016, 128, 'English', 'Ryan Gosling, Emma Stone', 'Damien Chazelle', 'PG-13', 'Some Language', 'Drama', 109),
('The Grand Budapest Hotel', 'A hotel concierge and lobby boy become entangled in a mystery.', 2014, 99, 'English', 'Ralph Fiennes, Tony Revolori', 'Wes Anderson', 'R', 'Language, Thematic Material', 'Comedy', 109),
('Knives Out', 'A detective investigates a wealthy familys suspicious death.', 2019, 130, 'English', 'Daniel Craig, Ana de Armas', 'Rian Johnson', 'PG-13', 'Thematic Elements', 'Mystery', 119),
('Glass Onion', 'Benoit Blanc solves a new mystery on a private island.', 2022, 139, 'English', 'Daniel Craig, Janelle Monae', 'Rian Johnson', 'PG-13', 'Language, Thematic Elements', 'Mystery', 119),
('No Time To Die', 'James Bond returns for one final mission.', 2021, 163, 'English', 'Daniel Craig, Lea Seydoux', 'Cary Joji Fukunaga', 'PG-13', 'Action Violence', 'Action', 129),
('Skyfall', 'Bond must defend MI6 when a cyberterrorist attacks.', 2012, 143, 'English', 'Daniel Craig, Javier Bardem', 'Sam Mendes', 'PG-13', 'Action, Violence', 'Action', 119),
('Casino Royale', 'A newly promoted Bond enters a high-stakes poker game.', 2006, 144, 'English', 'Daniel Craig, Eva Green', 'Martin Campbell', 'PG-13', 'Action Violence', 'Action', 119),
('The Lord of the Rings The Fellowship of the Ring', 'A hobbit begins a quest to destroy a powerful ring.', 2001, 178, 'English', 'Elijah Wood, Ian McKellen', 'Peter Jackson', 'PG-13', 'Fantasy Violence', 'Fantasy', 129),
('The Lord of the Rings The Two Towers', 'The fellowship is broken and wars rise across Middle-earth.', 2002, 179, 'English', 'Elijah Wood, Viggo Mortensen', 'Peter Jackson', 'PG-13', 'Fantasy Violence', 'Fantasy', 129),
('The Lord of the Rings The Return of the King', 'The final battle for Middle-earth decides the fate of all.', 2003, 201, 'English', 'Elijah Wood, Viggo Mortensen', 'Peter Jackson', 'PG-13', 'Fantasy Violence', 'Fantasy', 139),
('Harry Potter and the Sorcerers Stone', 'Harry discovers he is a wizard and attends Hogwarts.', 2001, 152, 'English', 'Daniel Radcliffe, Emma Watson', 'Chris Columbus', 'PG', 'Fantasy Peril', 'Fantasy', 109),
('Harry Potter and the Deathly Hallows Part 2', 'Harry faces Voldemort in the final Hogwarts battle.', 2011, 130, 'English', 'Daniel Radcliffe, Emma Watson', 'David Yates', 'PG-13', 'Fantasy Violence', 'Fantasy', 119);

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
    0 AS Rating,
    ms.CastCsv,
    ms.Director,
    ms.ContentRating,
    ms.ContentAdvisory,
    0 AS RentalCount,
    0 AS MyProperty,
    CONCAT('https://picsum.photos/seed/', LOWER(REPLACE(REPLACE(ms.Title, ' ', '-'), '.', '')), '/400/240') AS PosterPath,
    CONCAT('https://www.youtube.com/results?search_query=', REPLACE(ms.Title, ' ', '+'), '+official+trailer') AS TrailerUrl
FROM @MovieSeed ms
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Movies m WHERE m.Title = ms.Title
);

-- 4) Map Movie -> Genre in many-to-many table GenreMovie(GenresId, MoviesId)
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

-- 5) Seed one inventory row per movie (if absent)
INSERT INTO dbo.Inventories (MovieId, RentalPrice, IsAvailable)
SELECT m.Id, ms.RentalPrice, 1
FROM @MovieSeed ms
INNER JOIN dbo.Movies m ON m.Title = ms.Title
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Inventories i WHERE i.MovieId = m.Id
);

COMMIT TRANSACTION;
PRINT 'Seed complete: genres, 50 movies, movie-genre links, inventories.';
