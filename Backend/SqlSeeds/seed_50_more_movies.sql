SET NOCOUNT ON;
BEGIN TRANSACTION;

-- Add 50 more movies and their inventories (additive - does not delete existing data)
-- Ensure Genres exist (in case this runs on fresh DB)
INSERT INTO dbo.Genres (Name, [Description])
SELECT gs.Name, gs.[Description]
FROM (VALUES
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
    ('Thriller', 'Suspense-driven and tense narratives')
) AS gs(Name, [Description])
WHERE NOT EXISTS (SELECT 1 FROM dbo.Genres g WHERE g.Name = gs.Name);

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

INSERT INTO @MovieSeed (Title,[Description],ReleaseYear,DurationMinutes,[Language],CastCsv,Director,ContentRating,ContentAdvisory,GenreName,RentalPrice) VALUES
('Pulp Fiction','The lives of criminals intersect in a non-linear tale of crime and redemption.',1994,154,'English','John Travolta, Samuel L. Jackson','Quentin Tarantino','R','Violence, Language','Crime',119),
('Schindler''s List','A German industrialist saves over a thousand Jewish lives during the Holocaust.',1993,195,'English','Liam Neeson, Ralph Fiennes','Steven Spielberg','R','Mature Themes, Violence','Drama',129),
('The Silence of the Lambs','A young FBI agent must receive help from an imprisoned cannibal to catch another killer.',1991,118,'English','Jodie Foster, Anthony Hopkins','Jonathan Demme','R','Violence, Disturbing Images','Thriller',119),
('Back to the Future','A teenager is sent back in time to ensure his parents meet.',1985,116,'English','Michael J. Fox, Christopher Lloyd','Robert Zemeckis','PG','Sci-Fi Adventure','Sci-Fi',99),
('Terminator 2: Judgment Day','A cyborg must protect a young boy from a more advanced killer robot.',1991,137,'English','Arnold Schwarzenegger, Linda Hamilton','James Cameron','R','Violence, Language','Sci-Fi',119),
('Jurassic Park','Scientists create a theme park with cloned dinosaurs.',1993,127,'English','Sam Neill, Laura Dern','Steven Spielberg','PG-13','Intense Scenes','Adventure',109),
('Titanic','A love story aboard the doomed RMS Titanic.',1997,194,'English','Leonardo DiCaprio, Kate Winslet','James Cameron','PG-13','Disaster, Romance','Romance',119),
('Avatar','A paraplegic marine is dispatched to a moon with a unique ecosystem.',2009,162,'English','Sam Worthington, Zoe Saldana','James Cameron','PG-13','Intense Action','Sci-Fi',129),
('The Avengers','Earth''s mightiest heroes assemble to stop an alien invasion.',2012,143,'English','Robert Downey Jr., Chris Evans','Joss Whedon','PG-13','Violence, Action','Action',119),
('Black Panther','T''Challa returns to Wakanda to take his rightful place as king.',2018,134,'English','Chadwick Boseman, Michael B. Jordan','Ryan Coogler','PG-13','Violence, Thematic Elements','Action',119),
('Everything Everywhere All at Once','A woman discovers she can access parallel universes.',2022,139,'English','Michelle Yeoh, Ke Huy Quan','Daniel Kwan, Daniel Scheinert','R','Violence, Language','Sci-Fi',119),
('RRR','A fictional tale of two Indian revolutionaries and their friendship.',2022,182,'Telugu','N.T. Rama Rao Jr., Ram Charan','S.S. Rajamouli','U/A 16+','Violence, Action','Action',129),
('3 Idiots','Two friends search for their long-lost college roommate.',2009,170,'Hindi','Aamir Khan, Kareena Kapoor','Rajkumar Hirani','U/A 13+','Thematic Elements','Comedy',109),
('Baahubali: The Beginning','A man discovers his extraordinary destiny as the heir to a kingdom.',2015,159,'Telugu','Prabhas, Rana Daggubati','S.S. Rajamouli','U/A 16+','Epic Violence','Fantasy',119),
('Baahubali 2: The Conclusion','The story of Shivudu''s quest to rescue his love and reclaim the throne.',2017,167,'Telugu','Prabhas, Anushka Shetty','S.S. Rajamouli','U/A 16+','Epic Violence','Fantasy',129),
('Get Out','A young Black man uncovers a disturbing secret at his girlfriend''s family estate.',2017,104,'English','Daniel Kaluuya, Allison Williams','Jordan Peele','R','Violence, Horror','Horror',109),
('The Shape of Water','A mute woman forms a connection with an amphibian creature.',2017,123,'English','Sally Hawkins, Doug Jones','Guillermo del Toro','R','Adult Themes','Romance',109),
('La La Land','A jazz pianist and an aspiring actress pursue their dreams in Los Angeles.',2016,128,'English','Ryan Gosling, Emma Stone','Damien Chazelle','PG-13','Adult Themes','Romance',109),
('The Grand Budapest Hotel','The adventures of a legendary concierge at a famous European hotel.',2014,99,'English','Ralph Fiennes, Tony Revolori','Wes Anderson','R','Thematic Elements','Comedy',99),
('Spirited Away','A girl must work in a spirit world bathhouse to free her parents.',2001,125,'Japanese','Rumi Hiiragi, Miyu Irino','Hayao Miyazaki','PG','Fantasy Elements','Animation',109),
('Your Name','Two teenagers swap bodies and form a bond across time and space.',2016,106,'Japanese','Ryunosuke Kamiki, Mone Kamishiraishi','Makoto Shinkai','PG','Thematic Elements','Animation',99),
('Coco','A boy dreams of becoming a musician and enters the Land of the Dead.',2017,105,'English','Anthony Gonzalez, Gael García Bernal','Lee Unkrich','PG','Thematic Elements','Animation',99),
('The Departed','An undercover cop and a mole in the police try to identify each other.',2006,151,'English','Leonardo DiCaprio, Matt Damon','Martin Scorsese','R','Violence, Language','Crime',119),
('No Country for Old Men','A hunter stumbles upon drug money and is pursued by a relentless killer.',2007,122,'English','Javier Bardem, Josh Brolin','Joel and Ethan Coen','R','Violence, Language','Thriller',119),
('The Big Lebowski','A laid-back bowler gets caught up in a kidnapping scheme.',1998,117,'English','Jeff Bridges, John Goodman','Joel and Ethan Coen','R','Language, Drug Use','Comedy',99),
('Amélie','A shy waitress decides to change the lives of those around her for the better.',2001,122,'French','Audrey Tautou, Mathieu Kassovitz','Jean-Pierre Jeunet','R','Adult Themes','Romance',99),
('Oldboy','A man is inexplicably imprisoned for 15 years and seeks revenge upon release.',2003,120,'Korean','Choi Min-sik, Kang Hye-jeong','Park Chan-wook','R','Violence, Disturbing Images','Thriller',119),
('Train to Busan','Passengers on a train must survive a zombie outbreak.',2016,118,'Korean','Gong Yoo, Ma Dong-seok','Yeon Sang-ho','R','Violence, Horror','Horror',109),
('PK','An alien questions religious superstitions in India.',2014,153,'Hindi','Aamir Khan, Anushka Sharma','Rajkumar Hirani','U/A 13+','Thematic Elements','Comedy',109),
('Dangal','A former wrestler trains his daughters to become world-class wrestlers.',2016,161,'Hindi','Aamir Khan, Fatima Sana Shaikh','Nitesh Tiwari','U/A 13+','Sports Drama','Drama',119),
('Bajrangi Bhaijaan','A man with a heart of gold undertakes a journey to reunite a child with her family.',2015,163,'Hindi','Salman Khan, Kareena Kapoor','Kabir Khan','U/A 13+','Thematic Elements','Drama',109),
('Lagaan','Villagers challenge British rulers to a cricket match to avoid taxes.',2001,224,'Hindi','Aamir Khan, Gracy Singh','Ashutosh Gowariker','U/A 13+','Thematic Elements','Drama',119),
('KGF: Chapter 1','A young man rises to power in the gold mining underworld.',2018,156,'Kannada','Yash, Srinidhi Shetty','Prashanth Neel','U/A 16+','Violence','Action',119),
('KGF: Chapter 2','Rocky continues his quest for power and revenge.',2022,168,'Kannada','Yash, Sanjay Dutt','Prashanth Neel','U/A 16+','Violence','Action',129),
('Joker','A failed comedian descends into madness and becomes a criminal mastermind.',2019,122,'English','Joaquin Phoenix, Robert De Niro','Todd Phillips','R','Violence, Disturbing Images','Drama',119),
('Spider-Man: No Way Home','Peter Parker''s secret identity is revealed to the world.',2021,148,'English','Tom Holland, Zendaya','Jon Watts','PG-13','Action, Violence','Action',129),
('Top Gun: Maverick','Pete Mitchell trains a new generation of elite pilots.',2022,130,'English','Tom Cruise, Miles Teller','Joseph Kosinski','PG-13','Action, Intense Sequences','Action',129),
('Oppenheimer','The story of J. Robert Oppenheimer and the creation of the atomic bomb.',2023,180,'English','Cillian Murphy, Emily Blunt','Christopher Nolan','R','Mature Themes, Language','Drama',139),
('The Batman','Batman uncovers corruption in Gotham while hunting a serial killer.',2022,176,'English','Robert Pattinson, Zoë Kravitz','Matt Reeves','PG-13','Violence, Dark Themes','Action',129),
('Dune','A noble family becomes embroiled in a war for the most valuable asset in the galaxy.',2021,155,'English','Timothée Chalamet, Rebecca Ferguson','Denis Villeneuve','PG-13','Violence, Intense Scenes','Sci-Fi',129),
('Knives Out','A detective investigates the death of a wealthy novelist.',2019,130,'English','Daniel Craig, Ana de Armas','Rian Johnson','PG-13','Thematic Elements','Mystery',109),
('Once Upon a Time in Hollywood','A fading actor and his stunt double navigate 1969 Los Angeles.',2019,161,'English','Leonardo DiCaprio, Brad Pitt','Quentin Tarantino','R','Violence, Language','Drama',119),
('1917','Two soldiers race against time to deliver a message that could save 1,600 lives.',2019,119,'English','George MacKay, Dean-Charles Chapman','Sam Mendes','R','War Violence','Drama',119),
('The Notebook','A poor yet passionate young man falls in love with a rich young woman.',2004,123,'English','Ryan Gosling, Rachel McAdams','Nick Cassavetes','PG-13','Thematic Elements','Romance',99),
('Memento','A man with short-term memory loss tries to find his wife''s killer.',2000,113,'English','Guy Pearce, Carrie-Anne Moss','Christopher Nolan','R','Violence, Language','Mystery',109),
('Eternal Sunshine of the Spotless Mind','A couple undergo a procedure to erase each other from their memories.',2004,108,'English','Jim Carrey, Kate Winslet','Michel Gondry','R','Adult Themes','Romance',109),
('Requiem for a Dream','The drug-addled lives of four individuals spiral into desperation.',2000,102,'English','Ellen Burstyn, Jared Leto','Darren Aronofsky','R','Drug Use, Disturbing Images','Drama',109),
('The Truman Show','A man discovers his entire life is a televised reality show.',1998,103,'English','Jim Carrey, Laura Linney','Peter Weir','PG','Thematic Elements','Drama',99),
('Shutter Island','A U.S. Marshal investigates the disappearance of a patient from a hospital for the criminally insane.',2010,138,'English','Leonardo DiCaprio, Mark Ruffalo','Martin Scorsese','R','Violence, Disturbing Images','Thriller',119),
('Gone Girl','A man becomes the prime suspect when his wife goes missing.',2014,149,'English','Ben Affleck, Rosamund Pike','David Fincher','R','Violence, Language','Thriller',119);

-- Insert movies (skip if title already exists)
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
    CONCAT('https://picsum.photos/seed/', LOWER(REPLACE(REPLACE(ms.Title, ' ', '-'), '''', '')), '/400/240'),
    CONCAT('https://www.youtube.com/results?search_query=', REPLACE(ms.Title, ' ', '+'), '+official+trailer')
FROM @MovieSeed ms
WHERE NOT EXISTS (SELECT 1 FROM dbo.Movies m WHERE m.Title = ms.Title);

-- Link genres (GenreMovie)
INSERT INTO dbo.GenreMovie (GenresId, MoviesId)
SELECT g.Id, m.Id
FROM @MovieSeed ms
INNER JOIN dbo.Genres g ON g.Name = ms.GenreName
INNER JOIN dbo.Movies m ON m.Title = ms.Title
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.GenreMovie gm
    WHERE gm.GenresId = g.Id AND gm.MoviesId = m.Id
);

-- Add inventories (one per movie; skip if movie already has inventory)
INSERT INTO dbo.Inventories (MovieId, RentalPrice, IsAvailable)
SELECT m.Id, ms.RentalPrice, 1
FROM @MovieSeed ms
INNER JOIN dbo.Movies m ON m.Title = ms.Title
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Inventories i WHERE i.MovieId = m.Id
);

COMMIT TRANSACTION;
PRINT 'Added 50 more movies with genres and inventories.';
GO
