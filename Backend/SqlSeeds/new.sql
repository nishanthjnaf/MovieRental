SET NOCOUNT ON;
BEGIN TRANSACTION;

-- ---------------------------------------------------------------------------
-- 0) Wipe all application data (FK-safe order; includes Users)
-- ---------------------------------------------------------------------------
IF OBJECT_ID('dbo.RentalItems', 'U') IS NOT NULL DELETE FROM dbo.RentalItems;
IF OBJECT_ID('dbo.Payments', 'U') IS NOT NULL DELETE FROM dbo.Payments;
IF OBJECT_ID('dbo.Rentals', 'U') IS NOT NULL DELETE FROM dbo.Rentals;
IF OBJECT_ID('dbo.Reviews', 'U') IS NOT NULL DELETE FROM dbo.Reviews;
IF OBJECT_ID('dbo.Watchlists', 'U') IS NOT NULL DELETE FROM dbo.Watchlists;
IF OBJECT_ID('dbo.GenreMovie', 'U') IS NOT NULL DELETE FROM dbo.GenreMovie;
IF OBJECT_ID('dbo.Inventories', 'U') IS NOT NULL DELETE FROM dbo.Inventories;
IF OBJECT_ID('dbo.Movies', 'U') IS NOT NULL DELETE FROM dbo.Movies;
IF OBJECT_ID('dbo.Genres', 'U') IS NOT NULL DELETE FROM dbo.Genres;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DELETE FROM dbo.Users;

IF OBJECT_ID('dbo.RentalItems', 'U') IS NOT NULL DBCC CHECKIDENT ('dbo.RentalItems', RESEED, 0);
IF OBJECT_ID('dbo.Payments', 'U') IS NOT NULL DBCC CHECKIDENT ('dbo.Payments', RESEED, 0);
IF OBJECT_ID('dbo.Rentals', 'U') IS NOT NULL DBCC CHECKIDENT ('dbo.Rentals', RESEED, 0);
IF OBJECT_ID('dbo.Reviews', 'U') IS NOT NULL DBCC CHECKIDENT ('dbo.Reviews', RESEED, 0);
IF OBJECT_ID('dbo.Watchlists', 'U') IS NOT NULL DBCC CHECKIDENT ('dbo.Watchlists', RESEED, 0);
IF OBJECT_ID('dbo.Inventories', 'U') IS NOT NULL DBCC CHECKIDENT ('dbo.Inventories', RESEED, 0);
IF OBJECT_ID('dbo.Movies', 'U') IS NOT NULL DBCC CHECKIDENT ('dbo.Movies', RESEED, 0);
IF OBJECT_ID('dbo.Genres', 'U') IS NOT NULL DBCC CHECKIDENT ('dbo.Genres', RESEED, 0);
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DBCC CHECKIDENT ('dbo.Users', RESEED, 0);

-- ---------------------------------------------------------------------------
-- Optional: defaults for nullable/legacy columns (matches other seed scripts)
-- ---------------------------------------------------------------------------
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

-- ---------------------------------------------------------------------------
-- 1) Genres
-- ---------------------------------------------------------------------------
INSERT INTO dbo.Genres (Name, [Description]) VALUES
('Action', 'High-energy films with stunts and combat'),
('Adventure', 'Exploration, quests, and epic journeys'),
('Animation', 'Animated storytelling for all ages'),
('Comedy', 'Lighthearted and humorous films'),
('Crime', 'Stories centered on criminal acts and justice'),
('Drama', 'Character-driven emotional narratives'),
('Fantasy', 'Magic, mythical worlds, and supernatural elements'),
('Horror', 'Fear and tension focused films'),
('Mystery', 'Investigative and puzzle-driven stories'),
('Romance', 'Love and relationships'),
('Sci-Fi', 'Science and futuristic speculative storytelling'),
('Thriller', 'Suspense-driven and tense narratives');

-- ---------------------------------------------------------------------------
-- 2) 50 real movies + two inventory price tiers per title
-- ---------------------------------------------------------------------------
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
    RentalPriceTier1 REAL,
    RentalPriceTier2 REAL
);

INSERT INTO @MovieSeed (
    Title, [Description], ReleaseYear, DurationMinutes, [Language], CastCsv, Director,
    ContentRating, ContentAdvisory, GenreName, RentalPriceTier1, RentalPriceTier2
) VALUES
('The Shawshank Redemption','Two imprisoned men bond over years, finding solace and eventual redemption.',1994,142,'English','Tim Robbins, Morgan Freeman','Frank Darabont','R','Mature Themes, Language','Drama',99,119),
('The Godfather','The aging patriarch of an organized crime dynasty transfers control to his reluctant son.',1972,175,'English','Marlon Brando, Al Pacino','Francis Ford Coppola','R','Violence, Language','Crime',129,149),
('The Dark Knight','Batman faces the Joker in a battle for Gotham''s soul.',2008,152,'English','Christian Bale, Heath Ledger','Christopher Nolan','PG-13','Violence, Intense Sequences','Action',119,139),
('Inception','A thief who steals corporate secrets through dream-sharing technology is given an inverse task.',2010,148,'English','Leonardo DiCaprio, Joseph Gordon-Levitt','Christopher Nolan','PG-13','Violence, Intense Scenes','Sci-Fi',109,134),
('Interstellar','A team travels through a wormhole in space in an attempt to ensure humanity''s survival.',2014,169,'English','Matthew McConaughey, Anne Hathaway','Christopher Nolan','PG-13','Intense Scenes','Sci-Fi',124,149),
('Parasite','Greed and class discrimination threaten a newly formed symbiotic relationship.',2019,132,'Korean','Song Kang-ho, Choi Woo-shik','Bong Joon-ho','R','Violence, Language','Thriller',104,124),
('Fight Club','An office worker forms an underground fight club.',1999,139,'English','Brad Pitt, Edward Norton','David Fincher','R','Violence, Language','Drama',94,114),
('Forrest Gump','The life journey of Forrest Gump across historic moments.',1994,142,'English','Tom Hanks, Robin Wright','Robert Zemeckis','PG-13','Thematic Elements','Drama',99,119),
('The Matrix','A hacker discovers the shocking truth about reality.',1999,136,'English','Keanu Reeves, Laurence Fishburne','The Wachowskis','R','Sci-Fi Violence','Sci-Fi',109,129),
('The Lord of the Rings: The Fellowship of the Ring','A hobbit begins a quest to destroy a powerful ring.',2001,178,'English','Elijah Wood, Ian McKellen','Peter Jackson','PG-13','Fantasy Violence','Fantasy',119,144),
('The Lord of the Rings: The Two Towers','The fellowship is broken and wars rise across Middle-earth.',2002,179,'English','Elijah Wood, Viggo Mortensen','Peter Jackson','PG-13','Fantasy Violence','Fantasy',119,139),
('The Lord of the Rings: The Return of the King','The fate of Middle-earth is decided.',2003,201,'English','Elijah Wood, Viggo Mortensen','Peter Jackson','PG-13','Fantasy Violence','Fantasy',129,154),
('The Godfather Part II','The early life of Vito Corleone juxtaposed with Michael''s reign.',1974,202,'English','Al Pacino, Robert De Niro','Francis Ford Coppola','R','Violence, Language','Crime',129,149),
('The Green Mile','Death row guards are affected by one prisoner''s extraordinary gift.',1999,189,'English','Tom Hanks, Michael Clarke Duncan','Frank Darabont','R','Mature Themes','Drama',104,124),
('Gladiator','A Roman general seeks vengeance as a gladiator.',2000,155,'English','Russell Crowe, Joaquin Phoenix','Ridley Scott','R','Strong Violence','Action',114,134),
('Saving Private Ryan','A squad searches for a missing paratrooper in WWII.',1998,169,'English','Tom Hanks, Matt Damon','Steven Spielberg','R','War Violence','Drama',114,139),
('Se7en','Two detectives hunt a serial killer who uses the seven deadly sins.',1995,127,'English','Brad Pitt, Morgan Freeman','David Fincher','R','Violence, Disturbing Images','Thriller',109,129),
('Whiplash','A drummer is pushed beyond limits by an abusive instructor.',2014,106,'English','Miles Teller, J.K. Simmons','Damien Chazelle','R','Language, Intensity','Drama',94,114),
('The Prestige','Two rival magicians engage in a bitter competition.',2006,130,'English','Hugh Jackman, Christian Bale','Christopher Nolan','PG-13','Some Violence','Mystery',99,119),
('The Lion King','A young lion prince flees his kingdom only to learn the true meaning of responsibility.',1994,88,'English','Matthew Broderick, Jeremy Irons','Roger Allers','G','Mild Peril','Animation',89,104),
('Pulp Fiction','Interwoven crime stories collide in Los Angeles.',1994,154,'English','John Travolta, Samuel L. Jackson','Quentin Tarantino','R','Violence, Language, Drugs','Crime',109,129),
('Schindler''s List','A businessman saves Jewish workers during the Holocaust.',1993,195,'English','Liam Neeson, Ralph Fiennes','Steven Spielberg','R','Disturbing Images, Violence','Drama',119,144),
('Goodfellas','The rise and fall of a mob associate over decades.',1990,146,'English','Ray Liotta, Robert De Niro','Martin Scorsese','R','Violence, Language','Crime',104,124),
('The Silence of the Lambs','An FBI trainee seeks help from a brilliant cannibal to catch a killer.',1991,118,'English','Jodie Foster, Anthony Hopkins','Jonathan Demme','R','Violence, Disturbing Content','Thriller',99,119),
('Spirited Away','A girl enters a spirit world and must find a way to save her parents.',2001,125,'Japanese','Rumi Hiiragi, Miyu Irino','Hayao Miyazaki','PG','Mild Peril','Animation',99,119),
('Toy Story','Cowboy and space ranger toys learn to cooperate when a new toy arrives.',1995,81,'English','Tom Hanks, Tim Allen','John Lasseter','G','Mild Peril','Animation',84,99),
('Terminator 2: Judgment Day','A reprogrammed Terminator protects a boy from a liquid-metal assassin.',1991,137,'English','Arnold Schwarzenegger, Linda Hamilton','James Cameron','R','Sci-Fi Violence','Action',104,124),
('Aliens','Marines battle deadly creatures on a distant colony.',1986,137,'English','Sigourney Weaver, Michael Biehn','James Cameron','R','Sci-Fi Horror Violence','Sci-Fi',99,119),
('Back to the Future','A teen accidentally travels to 1955 and must get his parents together.',1985,116,'English','Michael J. Fox, Christopher Lloyd','Robert Zemeckis','PG','Mild Language','Adventure',89,109),
('Casablanca','An expatriate must choose between love and duty in wartime Morocco.',1942,102,'English','Humphrey Bogart, Ingrid Bergman','Michael Curtiz','PG','Mild Themes','Romance',79,94),
('Citizen Kane','The rise and fall of a newspaper magnate is told through flashbacks.',1941,119,'English','Orson Welles, Joseph Cotten','Orson Welles','PG','Thematic Elements','Drama',79,99),
('12 Angry Men','Jurors debate reasonable doubt in a murder trial.',1957,96,'English','Henry Fonda, Lee J. Cobb','Sidney Lumet','PG','Thematic Material','Drama',74,89),
('The Departed','An undercover cop and a mole infiltrate the Boston mob.',2006,151,'English','Leonardo DiCaprio, Matt Damon','Martin Scorsese','R','Violence, Language','Crime',114,139),
('Django Unchained','A freed slave teams with a bounty hunter to rescue his wife.',2012,165,'English','Jamie Foxx, Christoph Waltz','Quentin Tarantino','R','Violence, Language','Action',119,144),
('No Country for Old Men','A hunter stumbles on drug money and faces a relentless killer.',2007,122,'English','Tommy Lee Jones, Javier Bardem','Ethan Coen, Joel Coen','R','Strong Violence','Thriller',104,124),
('There Will Be Blood','An oil prospector''s ambition consumes everything around him.',2007,158,'English','Daniel Day-Lewis, Paul Dano','Paul Thomas Anderson','R','Violence, Language','Drama',114,139),
('Mad Max: Fury Road','In a wasteland, rebels flee a tyrant in a relentless chase.',2015,120,'English','Tom Hardy, Charlize Theron','George Miller','R','Strong Violence','Action',109,129),
('Dune: Part Two','Paul Atreides unites with the Fremen for revenge and destiny.',2024,166,'English','Timothée Chalamet, Zendaya','Denis Villeneuve','PG-13','Violence, Intense Scenes','Sci-Fi',129,154),
('Blade Runner 2049','A young blade runner uncovers a secret that could end society.',2017,164,'English','Ryan Gosling, Harrison Ford','Denis Villeneuve','R','Violence, Nudity','Sci-Fi',114,139),
('Dune','House Atreides faces betrayal on the desert planet Arrakis.',2021,155,'English','Timothée Chalamet, Rebecca Ferguson','Denis Villeneuve','PG-13','Violence, Intense Scenes','Sci-Fi',119,144),
('Spider-Man: Into the Spider-Verse','Teen Miles Morales becomes Spider-Man across dimensions.',2018,117,'English','Shameik Moore, Jake Johnson','Bob Persichetti','PG','Fantasy Action','Animation',104,124),
('Get Out','A young man visits his girlfriend''s family and uncovers a horrifying truth.',2017,104,'English','Daniel Kaluuya, Allison Williams','Jordan Peele','R','Violence, Language','Horror',99,119),
('Joker','A failed comedian''s descent into chaos reshapes Gotham.',2019,122,'English','Joaquin Phoenix, Robert De Niro','Todd Phillips','R','Violence, Disturbing Content','Thriller',109,129),
('Oppenheimer','The physicist leads the Manhattan Project amid moral crisis.',2023,180,'English','Cillian Murphy, Emily Blunt','Christopher Nolan','R','Sexuality, Language','Drama',129,159),
('Barbie','Barbie and Ken journey to the real world and question their existence.',2023,114,'English','Margot Robbie, Ryan Gosling','Greta Gerwig','PG-13','Suggestive Content','Comedy',104,124),
('Everything Everywhere All at Once','A laundromat owner must save the multiverse from chaos.',2022,139,'English','Michelle Yeoh, Ke Huy Quan','Daniel Kwan, Daniel Scheinert','R','Violence, Language','Sci-Fi',114,139),
('La La Land','Two artists in Los Angeles chase dreams and romance.',2016,128,'English','Ryan Gosling, Emma Stone','Damien Chazelle','PG-13','Thematic Elements','Romance',99,119),
('The Grand Budapest Hotel','A concierge and lobby boy become entangled in a European caper.',2014,99,'English','Ralph Fiennes, Tony Revolori','Wes Anderson','R','Violence, Language','Comedy',94,114),
('Knives Out','A detective investigates the death of a famous mystery novelist.',2019,130,'English','Daniel Craig, Ana de Armas','Rian Johnson','PG-13','Thematic Elements','Mystery',99,119),
('Gravity','Two astronauts fight to survive after debris destroys their shuttle.',2013,91,'English','Sandra Bullock, George Clooney','Alfonso Cuarón','PG-13','Peril, Thematic Elements','Thriller',104,124);

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
    CONCAT('https://picsum.photos/seed/', LOWER(REPLACE(REPLACE(ms.Title, ' ', '-'), ':', '')), '/400/240'),
    CONCAT('https://www.youtube.com/results?search_query=', REPLACE(ms.Title, ' ', '+'), '+official+trailer')
FROM @MovieSeed ms;

INSERT INTO dbo.GenreMovie (GenresId, MoviesId)
SELECT g.Id, m.Id
FROM @MovieSeed ms
INNER JOIN dbo.Genres g ON g.Name = ms.GenreName
INNER JOIN dbo.Movies m ON m.Title = ms.Title;

INSERT INTO dbo.Inventories (MovieId, RentalPrice, IsAvailable)
SELECT m.Id, ms.RentalPriceTier1, 1
FROM @MovieSeed ms
INNER JOIN dbo.Movies m ON m.Title = ms.Title;

INSERT INTO dbo.Inventories (MovieId, RentalPrice, IsAvailable)
SELECT m.Id, ms.RentalPriceTier2, 1
FROM @MovieSeed ms
INNER JOIN dbo.Movies m ON m.Title = ms.Title;

COMMIT TRANSACTION;
PRINT 'Wipe complete. Seeded 12 genres, 50 real movies, 100 inventory rows (2 prices per movie), and cleared all users/rentals.';
