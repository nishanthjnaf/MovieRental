-- ============================================================
-- Part 1: Update posters for 6 existing series
-- Part 2: Insert 10 new series with seasons and episodes
-- Run against your MovieRental database
-- ============================================================

BEGIN TRANSACTION;

-- ============================================================
-- PART 1: Poster placeholders for existing 6 series
-- (Run fetch_series_posters.js to get real URLs, then update)
-- ============================================================

UPDATE dbo.Series SET PosterPath = 'https://picsum.photos/seed/breaking-bad/500/750'    WHERE Title = 'Breaking Bad';
UPDATE dbo.Series SET PosterPath = 'https://picsum.photos/seed/stranger-things/500/750' WHERE Title = 'Stranger Things';
UPDATE dbo.Series SET PosterPath = 'https://picsum.photos/seed/the-crown/500/750'       WHERE Title = 'The Crown';
UPDATE dbo.Series SET PosterPath = 'https://picsum.photos/seed/chernobyl/500/750'       WHERE Title = 'Chernobyl';
UPDATE dbo.Series SET PosterPath = 'https://picsum.photos/seed/narcos/500/750'          WHERE Title = 'Narcos';
UPDATE dbo.Series SET PosterPath = 'https://picsum.photos/seed/the-boys/500/750'        WHERE Title = 'The Boys';

-- ============================================================
-- PART 2: Insert 10 new series
-- ============================================================

-- Helper to get genre id
-- We'll use a temp table approach per series

-- 1. Game of Thrones
IF NOT EXISTS (SELECT 1 FROM dbo.Series WHERE Title = 'Game of Thrones')
BEGIN
    INSERT INTO dbo.Series (Title, [Description], [Language], Director, Cast, ContentRating, ContentAdvisory, PosterPath, TrailerUrl, RentalCount, RentalPrice, IsAvailable)
    VALUES ('Game of Thrones',
        'Noble families fight for control of the Iron Throne while an ancient enemy rises beyond the Wall.',
        'English', 'David Benioff, D.B. Weiss',
        'Emilia Clarke, Kit Harington, Peter Dinklage',
        'TV-MA', 'Violence, Nudity, Language',
        'https://picsum.photos/seed/game-of-thrones/500/750',
        'https://www.youtube.com/results?search_query=Game+of+Thrones+official+trailer',
        0, 149, 1);

    DECLARE @got INT = SCOPE_IDENTITY();
    INSERT INTO dbo.SeriesGenres (SeriesId, GenresId) SELECT @got, Id FROM dbo.Genres WHERE Name = 'Drama';

    -- Season 1
    INSERT INTO dbo.Seasons (SeriesId, SeasonNumber, Title, ReleaseYear, AverageRating) VALUES (@got, 1, 'Season 1', 2011, 0);
    DECLARE @got_s1 INT = SCOPE_IDENTITY();
    INSERT INTO dbo.Episodes (SeasonId, EpisodeNumber, Title, [Description], DurationMinutes, AirDate) VALUES
    (@got_s1, 1, 'Winter Is Coming', 'Lord Stark is asked to serve the king.', 62, '2011-04-17'),
    (@got_s1, 2, 'The Kingsroad', 'The Starks head south to King''s Landing.', 56, '2011-04-24'),
    (@got_s1, 3, 'Lord Snow', 'Jon Snow arrives at the Wall.', 58, '2011-05-01');

    -- Season 2
    INSERT INTO dbo.Seasons (SeriesId, SeasonNumber, Title, ReleaseYear, AverageRating) VALUES (@got, 2, 'Season 2', 2012, 0);
    DECLARE @got_s2 INT = SCOPE_IDENTITY();
    INSERT INTO dbo.Episodes (SeasonId, EpisodeNumber, Title, [Description], DurationMinutes, AirDate) VALUES
    (@got_s2, 1, 'The North Remembers', 'Joffrey''s reign begins with cruelty.', 58, '2012-04-01'),
    (@got_s2, 2, 'The Night Lands', 'Arya travels north with new companions.', 54, '2012-04-08');
END;

-- 2. Dark
IF NOT EXISTS (SELECT 1 FROM dbo.Series WHERE Title = 'Dark')
BEGIN
    INSERT INTO dbo.Series (Title, [Description], [Language], Director, Cast, ContentRating, ContentAdvisory, PosterPath, TrailerUrl, RentalCount, RentalPrice, IsAvailable)
    VALUES ('Dark',
        'A family saga with a supernatural twist set in a German town where the disappearance of children exposes a time travel conspiracy.',
        'German', 'Baran bo Odar',
        'Louis Hofmann, Oliver Masucci, Lisa Vicari',
        'TV-MA', 'Violence, Thematic Elements',
        'https://picsum.photos/seed/dark-netflix/500/750',
        'https://www.youtube.com/results?search_query=Dark+Netflix+official+trailer',
        0, 119, 1);

    DECLARE @dark INT = SCOPE_IDENTITY();
    INSERT INTO dbo.SeriesGenres (SeriesId, GenresId) SELECT @dark, Id FROM dbo.Genres WHERE Name = 'Sci-Fi';

    INSERT INTO dbo.Seasons (SeriesId, SeasonNumber, Title, ReleaseYear, AverageRating) VALUES (@dark, 1, 'Season 1', 2017, 0);
    DECLARE @dark_s1 INT = SCOPE_IDENTITY();
    INSERT INTO dbo.Episodes (SeasonId, EpisodeNumber, Title, [Description], DurationMinutes, AirDate) VALUES
    (@dark_s1, 1, 'Secrets', 'Two children go missing in the small town of Winden.', 60, '2017-12-01'),
    (@dark_s1, 2, 'Lies', 'The police investigation deepens.', 60, '2017-12-01'),
    (@dark_s1, 3, 'Past and Present', 'A cave holds a mysterious passage.', 60, '2017-12-01');

    INSERT INTO dbo.Seasons (SeriesId, SeasonNumber, Title, ReleaseYear, AverageRating) VALUES (@dark, 2, 'Season 2', 2019, 0);
    DECLARE @dark_s2 INT = SCOPE_IDENTITY();
    INSERT INTO dbo.Episodes (SeasonId, EpisodeNumber, Title, [Description], DurationMinutes, AirDate) VALUES
    (@dark_s2, 1, 'Beginnings and Endings', 'The apocalypse approaches.', 60, '2019-06-21'),
    (@dark_s2, 2, 'Dark Matter', 'Time loops tighten around the families.', 60, '2019-06-21');
END;

-- 3. Peaky Blinders
IF NOT EXISTS (SELECT 1 FROM dbo.Series WHERE Title = 'Peaky Blinders')
BEGIN
    INSERT INTO dbo.Series (Title, [Description], [Language], Director, Cast, ContentRating, ContentAdvisory, PosterPath, TrailerUrl, RentalCount, RentalPrice, IsAvailable)
    VALUES ('Peaky Blinders',
        'A gangster family epic set in 1919 Birmingham, England, centered on the Shelby crime family.',
        'English', 'Steven Knight',
        'Cillian Murphy, Tom Hardy, Helen McCrory',
        'TV-MA', 'Violence, Language, Drug Use',
        'https://picsum.photos/seed/peaky-blinders/500/750',
        'https://www.youtube.com/results?search_query=Peaky+Blinders+official+trailer',
        0, 129, 1);

    DECLARE @pb INT = SCOPE_IDENTITY();
    INSERT INTO dbo.SeriesGenres (SeriesId, GenresId) SELECT @pb, Id FROM dbo.Genres WHERE Name = 'Crime';

    INSERT INTO dbo.Seasons (SeriesId, SeasonNumber, Title, ReleaseYear, AverageRating) VALUES (@pb, 1, 'Season 1', 2013, 0);
    DECLARE @pb_s1 INT = SCOPE_IDENTITY();
    INSERT INTO dbo.Episodes (SeasonId, EpisodeNumber, Title, [Description], DurationMinutes, AirDate) VALUES
    (@pb_s1, 1, 'Episode 1', 'Tommy Shelby runs the Peaky Blinders gang in post-war Birmingham.', 58, '2013-09-12'),
    (@pb_s1, 2, 'Episode 2', 'A police inspector arrives to dismantle the gang.', 58, '2013-09-19'),
    (@pb_s1, 3, 'Episode 3', 'Tommy makes a dangerous deal.', 58, '2013-09-26');

    INSERT INTO dbo.Seasons (SeriesId, SeasonNumber, Title, ReleaseYear, AverageRating) VALUES (@pb, 2, 'Season 2', 2014, 0);
    DECLARE @pb_s2 INT = SCOPE_IDENTITY();
    INSERT INTO dbo.Episodes (SeasonId, EpisodeNumber, Title, [Description], DurationMinutes, AirDate) VALUES
    (@pb_s2, 1, 'Episode 1', 'The Shelbys expand to London.', 58, '2014-10-02'),
    (@pb_s2, 2, 'Episode 2', 'New enemies emerge in the capital.', 58, '2014-10-09');
END;

-- 4. Money Heist (La Casa de Papel)
IF NOT EXISTS (SELECT 1 FROM dbo.Series WHERE Title = 'Money Heist')
BEGIN
    INSERT INTO dbo.Series (Title, [Description], [Language], Director, Cast, ContentRating, ContentAdvisory, PosterPath, TrailerUrl, RentalCount, RentalPrice, IsAvailable)
    VALUES ('Money Heist',
        'A criminal mastermind recruits eight thieves to carry out the greatest heist in history against the Royal Mint of Spain.',
        'Spanish', 'Álex Pina',
        'Álvaro Morte, Úrsula Corberó, Itziar Ituño',
        'TV-MA', 'Violence, Language',
        'https://picsum.photos/seed/money-heist/500/750',
        'https://www.youtube.com/results?search_query=Money+Heist+official+trailer',
        0, 119, 1);

    DECLARE @mh INT = SCOPE_IDENTITY();
    INSERT INTO dbo.SeriesGenres (SeriesId, GenresId) SELECT @mh, Id FROM dbo.Genres WHERE Name = 'Thriller';

    INSERT INTO dbo.Seasons (SeriesId, SeasonNumber, Title, ReleaseYear, AverageRating) VALUES (@mh, 1, 'Part 1', 2017, 0);
    DECLARE @mh_s1 INT = SCOPE_IDENTITY();
    INSERT INTO dbo.Episodes (SeasonId, EpisodeNumber, Title, [Description], DurationMinutes, AirDate) VALUES
    (@mh_s1, 1, 'Episode 1', 'The Professor recruits his team for the heist.', 70, '2017-05-02'),
    (@mh_s1, 2, 'Episode 2', 'The heist begins at the Royal Mint.', 70, '2017-05-02'),
    (@mh_s1, 3, 'Episode 3', 'Hostage negotiations begin.', 70, '2017-05-02');

    INSERT INTO dbo.Seasons (SeriesId, SeasonNumber, Title, ReleaseYear, AverageRating) VALUES (@mh, 2, 'Part 2', 2017, 0);
    DECLARE @mh_s2 INT = SCOPE_IDENTITY();
    INSERT INTO dbo.Episodes (SeasonId, EpisodeNumber, Title, [Description], DurationMinutes, AirDate) VALUES
    (@mh_s2, 1, 'Episode 1', 'The gang fights to complete the heist.', 70, '2017-11-03'),
    (@mh_s2, 2, 'Episode 2', 'The Professor''s plan unravels.', 70, '2017-11-03');
END;

-- 5. Mindhunter
IF NOT EXISTS (SELECT 1 FROM dbo.Series WHERE Title = 'Mindhunter')
BEGIN
    INSERT INTO dbo.Series (Title, [Description], [Language], Director, Cast, ContentRating, ContentAdvisory, PosterPath, TrailerUrl, RentalCount, RentalPrice, IsAvailable)
    VALUES ('Mindhunter',
        'FBI agents interview imprisoned serial killers to understand their psychology and solve ongoing cases.',
        'English', 'David Fincher',
        'Jonathan Groff, Holt McCallany, Anna Torv',
        'TV-MA', 'Violence, Language, Disturbing Content',
        'https://picsum.photos/seed/mindhunter/500/750',
        'https://www.youtube.com/results?search_query=Mindhunter+official+trailer',
        0, 119, 1);

    DECLARE @mhunt INT = SCOPE_IDENTITY();
    INSERT INTO dbo.SeriesGenres (SeriesId, GenresId) SELECT @mhunt, Id FROM dbo.Genres WHERE Name = 'Crime';

    INSERT INTO dbo.Seasons (SeriesId, SeasonNumber, Title, ReleaseYear, AverageRating) VALUES (@mhunt, 1, 'Season 1', 2017, 0);
    DECLARE @mhunt_s1 INT = SCOPE_IDENTITY();
    INSERT INTO dbo.Episodes (SeasonId, EpisodeNumber, Title, [Description], DurationMinutes, AirDate) VALUES
    (@mhunt_s1, 1, 'Episode 1', 'Agent Ford begins interviewing serial killers.', 60, '2017-10-13'),
    (@mhunt_s1, 2, 'Episode 2', 'The team profiles a new suspect.', 60, '2017-10-13'),
    (@mhunt_s1, 3, 'Episode 3', 'Patterns emerge across cases.', 60, '2017-10-13');

    INSERT INTO dbo.Seasons (SeriesId, SeasonNumber, Title, ReleaseYear, AverageRating) VALUES (@mhunt, 2, 'Season 2', 2019, 0);
    DECLARE @mhunt_s2 INT = SCOPE_IDENTITY();
    INSERT INTO dbo.Episodes (SeasonId, EpisodeNumber, Title, [Description], DurationMinutes, AirDate) VALUES
    (@mhunt_s2, 1, 'Episode 1', 'The Atlanta child murders investigation begins.', 60, '2019-08-16'),
    (@mhunt_s2, 2, 'Episode 2', 'Ford''s methods are questioned.', 60, '2019-08-16');
END;

-- 6. Squid Game
IF NOT EXISTS (SELECT 1 FROM dbo.Series WHERE Title = 'Squid Game')
BEGIN
    INSERT INTO dbo.Series (Title, [Description], [Language], Director, Cast, ContentRating, ContentAdvisory, PosterPath, TrailerUrl, RentalCount, RentalPrice, IsAvailable)
    VALUES ('Squid Game',
        'Hundreds of cash-strapped players accept an invitation to compete in children''s games for a prize with deadly consequences.',
        'Korean', 'Hwang Dong-hyuk',
        'Lee Jung-jae, Park Hae-soo, Wi Ha-jun',
        'TV-MA', 'Strong Violence, Language',
        'https://picsum.photos/seed/squid-game/500/750',
        'https://www.youtube.com/results?search_query=Squid+Game+official+trailer',
        0, 119, 1);

    DECLARE @sg INT = SCOPE_IDENTITY();
    INSERT INTO dbo.SeriesGenres (SeriesId, GenresId) SELECT @sg, Id FROM dbo.Genres WHERE Name = 'Thriller';

    INSERT INTO dbo.Seasons (SeriesId, SeasonNumber, Title, ReleaseYear, AverageRating) VALUES (@sg, 1, 'Season 1', 2021, 0);
    DECLARE @sg_s1 INT = SCOPE_IDENTITY();
    INSERT INTO dbo.Episodes (SeasonId, EpisodeNumber, Title, [Description], DurationMinutes, AirDate) VALUES
    (@sg_s1, 1, 'Red Light, Green Light', 'Players enter the deadly competition.', 60, '2021-09-17'),
    (@sg_s1, 2, 'Hell', 'Survivors vote on whether to continue.', 63, '2021-09-17'),
    (@sg_s1, 3, 'The Man with the Umbrella', 'The marble game begins.', 56, '2021-09-17');

    INSERT INTO dbo.Seasons (SeriesId, SeasonNumber, Title, ReleaseYear, AverageRating) VALUES (@sg, 2, 'Season 2', 2024, 0);
    DECLARE @sg_s2 INT = SCOPE_IDENTITY();
    INSERT INTO dbo.Episodes (SeasonId, EpisodeNumber, Title, [Description], DurationMinutes, AirDate) VALUES
    (@sg_s2, 1, 'Episode 1', 'Gi-hun returns to find the games.', 60, '2024-12-26'),
    (@sg_s2, 2, 'Episode 2', 'New players, new games.', 60, '2024-12-26');
END;

-- 7. The Wire
IF NOT EXISTS (SELECT 1 FROM dbo.Series WHERE Title = 'The Wire')
BEGIN
    INSERT INTO dbo.Series (Title, [Description], [Language], Director, Cast, ContentRating, ContentAdvisory, PosterPath, TrailerUrl, RentalCount, RentalPrice, IsAvailable)
    VALUES ('The Wire',
        'The Baltimore drug scene is examined from the perspectives of law enforcement and drug dealers.',
        'English', 'David Simon',
        'Dominic West, Idris Elba, Lance Reddick',
        'TV-MA', 'Violence, Language, Drug Use',
        'https://picsum.photos/seed/the-wire/500/750',
        'https://www.youtube.com/results?search_query=The+Wire+official+trailer',
        0, 119, 1);

    DECLARE @wire INT = SCOPE_IDENTITY();
    INSERT INTO dbo.SeriesGenres (SeriesId, GenresId) SELECT @wire, Id FROM dbo.Genres WHERE Name = 'Crime';

    INSERT INTO dbo.Seasons (SeriesId, SeasonNumber, Title, ReleaseYear, AverageRating) VALUES (@wire, 1, 'Season 1', 2002, 0);
    DECLARE @wire_s1 INT = SCOPE_IDENTITY();
    INSERT INTO dbo.Episodes (SeasonId, EpisodeNumber, Title, [Description], DurationMinutes, AirDate) VALUES
    (@wire_s1, 1, 'The Target', 'Detective McNulty builds a case against drug lord Avon Barksdale.', 59, '2002-06-02'),
    (@wire_s1, 2, 'The Detail', 'The detail is assembled.', 59, '2002-06-09'),
    (@wire_s1, 3, 'The Buys', 'Undercover operations begin.', 59, '2002-06-16');

    INSERT INTO dbo.Seasons (SeriesId, SeasonNumber, Title, ReleaseYear, AverageRating) VALUES (@wire, 2, 'Season 2', 2003, 0);
    DECLARE @wire_s2 INT = SCOPE_IDENTITY();
    INSERT INTO dbo.Episodes (SeasonId, EpisodeNumber, Title, [Description], DurationMinutes, AirDate) VALUES
    (@wire_s2, 1, 'Ebb Tide', 'The investigation shifts to the docks.', 59, '2003-06-01'),
    (@wire_s2, 2, 'Collateral Damage', 'Dead bodies surface at the port.', 59, '2003-06-08');
END;

-- 8. Succession
IF NOT EXISTS (SELECT 1 FROM dbo.Series WHERE Title = 'Succession')
BEGIN
    INSERT INTO dbo.Series (Title, [Description], [Language], Director, Cast, ContentRating, ContentAdvisory, PosterPath, TrailerUrl, RentalCount, RentalPrice, IsAvailable)
    VALUES ('Succession',
        'The Roy family fights for control of their global media empire as their aging patriarch considers his future.',
        'English', 'Jesse Armstrong',
        'Brian Cox, Jeremy Strong, Sarah Snook',
        'TV-MA', 'Language, Sexual Content, Violence',
        'https://picsum.photos/seed/succession/500/750',
        'https://www.youtube.com/results?search_query=Succession+official+trailer',
        0, 129, 1);

    DECLARE @succ INT = SCOPE_IDENTITY();
    INSERT INTO dbo.SeriesGenres (SeriesId, GenresId) SELECT @succ, Id FROM dbo.Genres WHERE Name = 'Drama';

    INSERT INTO dbo.Seasons (SeriesId, SeasonNumber, Title, ReleaseYear, AverageRating) VALUES (@succ, 1, 'Season 1', 2018, 0);
    DECLARE @succ_s1 INT = SCOPE_IDENTITY();
    INSERT INTO dbo.Episodes (SeasonId, EpisodeNumber, Title, [Description], DurationMinutes, AirDate) VALUES
    (@succ_s1, 1, 'Celebration', 'Logan Roy''s 80th birthday party reveals family tensions.', 60, '2018-06-03'),
    (@succ_s1, 2, 'Shit Show at the Fuck Factory', 'Logan suffers a health crisis.', 60, '2018-06-10'),
    (@succ_s1, 3, 'Lifeboats', 'The siblings scramble for power.', 60, '2018-06-17');

    INSERT INTO dbo.Seasons (SeriesId, SeasonNumber, Title, ReleaseYear, AverageRating) VALUES (@succ, 2, 'Season 2', 2019, 0);
    DECLARE @succ_s2 INT = SCOPE_IDENTITY();
    INSERT INTO dbo.Episodes (SeasonId, EpisodeNumber, Title, [Description], DurationMinutes, AirDate) VALUES
    (@succ_s2, 1, 'The Summer Palace', 'The family regroups after the proxy fight.', 60, '2019-08-11'),
    (@succ_s2, 2, 'Vaulter', 'Logan targets a media acquisition.', 60, '2019-08-18');
END;

-- 9. Ozark
IF NOT EXISTS (SELECT 1 FROM dbo.Series WHERE Title = 'Ozark')
BEGIN
    INSERT INTO dbo.Series (Title, [Description], [Language], Director, Cast, ContentRating, ContentAdvisory, PosterPath, TrailerUrl, RentalCount, RentalPrice, IsAvailable)
    VALUES ('Ozark',
        'A financial advisor is forced to relocate his family to the Ozarks after a money-laundering scheme goes wrong.',
        'English', 'Bill Dubuque',
        'Jason Bateman, Laura Linney, Julia Garner',
        'TV-MA', 'Violence, Language',
        'https://picsum.photos/seed/ozark/500/750',
        'https://www.youtube.com/results?search_query=Ozark+official+trailer',
        0, 119, 1);

    DECLARE @oz INT = SCOPE_IDENTITY();
    INSERT INTO dbo.SeriesGenres (SeriesId, GenresId) SELECT @oz, Id FROM dbo.Genres WHERE Name = 'Thriller';

    INSERT INTO dbo.Seasons (SeriesId, SeasonNumber, Title, ReleaseYear, AverageRating) VALUES (@oz, 1, 'Season 1', 2017, 0);
    DECLARE @oz_s1 INT = SCOPE_IDENTITY();
    INSERT INTO dbo.Episodes (SeasonId, EpisodeNumber, Title, [Description], DurationMinutes, AirDate) VALUES
    (@oz_s1, 1, 'Sugarwood', 'Marty Byrde moves his family to the Ozarks.', 60, '2017-07-21'),
    (@oz_s1, 2, 'Blue Cat', 'Marty begins laundering money through local businesses.', 60, '2017-07-21'),
    (@oz_s1, 3, 'My Dripping Sleep', 'The Byrdes face local threats.', 60, '2017-07-21');

    INSERT INTO dbo.Seasons (SeriesId, SeasonNumber, Title, ReleaseYear, AverageRating) VALUES (@oz, 2, 'Season 2', 2018, 0);
    DECLARE @oz_s2 INT = SCOPE_IDENTITY();
    INSERT INTO dbo.Episodes (SeasonId, EpisodeNumber, Title, [Description], DurationMinutes, AirDate) VALUES
    (@oz_s2, 1, 'Reparations', 'The cartel tightens its grip.', 60, '2018-08-31'),
    (@oz_s2, 2, 'The Precious Blood of Jesus', 'A new threat emerges.', 60, '2018-08-31');
END;

-- 10. Scam 1992
IF NOT EXISTS (SELECT 1 FROM dbo.Series WHERE Title = 'Scam 1992')
BEGIN
    INSERT INTO dbo.Series (Title, [Description], [Language], Director, Cast, ContentRating, ContentAdvisory, PosterPath, TrailerUrl, RentalCount, RentalPrice, IsAvailable)
    VALUES ('Scam 1992',
        'The story of Harshad Mehta, a stockbroker who single-handedly took the stock market to dizzying heights and caused its biggest crash.',
        'Hindi', 'Hansal Mehta',
        'Pratik Gandhi, Shreya Dhanwanthary',
        'TV-14', 'Thematic Elements, Language',
        'https://picsum.photos/seed/scam-1992/500/750',
        'https://www.youtube.com/results?search_query=Scam+1992+official+trailer',
        0, 109, 1);

    DECLARE @scam INT = SCOPE_IDENTITY();
    INSERT INTO dbo.SeriesGenres (SeriesId, GenresId) SELECT @scam, Id FROM dbo.Genres WHERE Name = 'Drama';

    INSERT INTO dbo.Seasons (SeriesId, SeasonNumber, Title, ReleaseYear, AverageRating) VALUES (@scam, 1, 'Season 1', 2020, 0);
    DECLARE @scam_s1 INT = SCOPE_IDENTITY();
    INSERT INTO dbo.Episodes (SeasonId, EpisodeNumber, Title, [Description], DurationMinutes, AirDate) VALUES
    (@scam_s1, 1, 'The Hungry Young Man', 'Harshad Mehta arrives in Mumbai with big dreams.', 50, '2020-10-09'),
    (@scam_s1, 2, 'The Jobber', 'Harshad learns the stock market from the ground up.', 50, '2020-10-09'),
    (@scam_s1, 3, 'The Bull', 'Harshad begins his rise in the market.', 50, '2020-10-09'),
    (@scam_s1, 4, 'The Bear', 'The market turns against Harshad.', 50, '2020-10-09'),
    (@scam_s1, 5, 'The Broker', 'Harshad finds a new strategy.', 50, '2020-10-09');
END;

COMMIT TRANSACTION;
PRINT 'Series seed complete: 6 existing posters updated, 10 new series inserted.';
