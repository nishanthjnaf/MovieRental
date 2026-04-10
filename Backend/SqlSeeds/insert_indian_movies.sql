-- ============================================================
-- Insert 20 Indian Movies (Hindi, Tamil, Telugu, Malayalam)
-- Run against your MovieRental database
-- ============================================================

BEGIN TRANSACTION;

-- Ensure required genres exist
INSERT INTO dbo.Genres (Name, [Description])
SELECT g.Name, g.[Description]
FROM (VALUES
  ('Drama',    'Emotionally driven narratives'),
  ('Action',   'High-energy films with stunts and combat'),
  ('Thriller', 'Suspense-driven and tense narratives'),
  ('Crime',    'Stories involving criminal activities'),
  ('Romance',  'Love stories and relationships'),
  ('Comedy',   'Light-hearted and humorous films'),
  ('Mystery',  'Puzzles, secrets and investigations'),
  ('Sci-Fi',   'Science fiction and futuristic stories')
) AS g(Name, [Description])
WHERE NOT EXISTS (SELECT 1 FROM dbo.Genres ex WHERE ex.Name = g.Name);

DECLARE @IndianMovies TABLE (
    Title           NVARCHAR(200),
    [Description]   NVARCHAR(600),
    ReleaseYear     INT,
    DurationMinutes INT,
    [Language]      NVARCHAR(40),
    CastCsv         NVARCHAR(500),
    Director        NVARCHAR(120),
    ContentRating   NVARCHAR(20),
    ContentAdvisory NVARCHAR(200),
    GenreName       NVARCHAR(80),
    RentalPrice     REAL
);

INSERT INTO @IndianMovies VALUES
-- HINDI
('Dangal',
 'A former wrestler trains his daughters to become world-class wrestlers against all odds.',
 2016, 161, 'Hindi', 'Aamir Khan, Fatima Sana Shaikh', 'Nitesh Tiwari',
 'U', 'Mild Thematic Elements', 'Drama', 99),

('3 Idiots',
 'Three engineering students challenge the education system and find their true calling.',
 2009, 170, 'Hindi', 'Aamir Khan, R. Madhavan, Sharman Joshi', 'Rajkumar Hirani',
 'U', 'Mild Language', 'Comedy', 89),

('Lagaan',
 'Villagers in colonial India challenge British officers to a cricket match to avoid taxes.',
 2001, 224, 'Hindi', 'Aamir Khan, Gracy Singh', 'Ashutosh Gowariker',
 'U', 'Mild Thematic Elements', 'Drama', 94),

('Gangs of Wasseypur',
 'A saga of crime, revenge and power spanning three generations in a coal mining town.',
 2012, 160, 'Hindi', 'Manoj Bajpayee, Nawazuddin Siddiqui', 'Anurag Kashyap',
 'A', 'Strong Violence, Language', 'Crime', 109),

('Andhadhun',
 'A blind pianist becomes entangled in a murder mystery with dark twists.',
 2018, 139, 'Hindi', 'Ayushmann Khurrana, Tabu', 'Sriram Raghavan',
 'U/A', 'Violence, Thematic Elements', 'Thriller', 99),

('Dil Chahta Hai',
 'Three best friends navigate love, life and friendship after college.',
 2001, 183, 'Hindi', 'Aamir Khan, Saif Ali Khan, Akshaye Khanna', 'Farhan Akhtar',
 'U', 'Mild Language', 'Comedy', 84),

-- TAMIL
('Vikram',
 'A special agent investigates a series of murders linked to a masked vigilante group.',
 2022, 174, 'Tamil', 'Kamal Haasan, Vijay Sethupathi, Fahadh Faasil', 'Lokesh Kanagaraj',
 'U/A', 'Strong Violence, Language', 'Action', 119),

('Vinnaithaandi Varuvaayaa',
 'A young man falls for a Christian girl and pursues her despite family opposition.',
 2010, 155, 'Tamil', 'Silambarasan, Trisha Krishnan', 'Gautham Vasudev Menon',
 'U', 'Mild Thematic Elements', 'Romance', 84),

('Anbe Sivam',
 'Two strangers stranded together discover the meaning of love and humanity.',
 2003, 166, 'Tamil', 'Kamal Haasan, R. Madhavan', 'Sundar C',
 'U', 'Mild Thematic Elements', 'Drama', 84),

('Kaithi',
 'An ex-convict fights through a night of chaos to meet his daughter for the first time.',
 2019, 145, 'Tamil', 'Karthi, Narain', 'Lokesh Kanagaraj',
 'U/A', 'Strong Violence', 'Action', 104),

('96',
 'Two former classmates reunite after decades and revisit their unspoken love.',
 2018, 158, 'Tamil', 'Vijay Sethupathi, Trisha Krishnan', 'C. Prem Kumar',
 'U', 'Mild Thematic Elements', 'Romance', 89),

-- TELUGU
('Baahubali: The Beginning',
 'A foundling discovers his royal heritage and fights to reclaim his kingdom.',
 2015, 159, 'Telugu', 'Prabhas, Rana Daggubati', 'S. S. Rajamouli',
 'U/A', 'Fantasy Violence', 'Action', 119),

('Baahubali 2: The Conclusion',
 'The truth behind the king''s betrayal is revealed in an epic battle for justice.',
 2017, 167, 'Telugu', 'Prabhas, Anushka Shetty', 'S. S. Rajamouli',
 'U/A', 'Fantasy Violence', 'Action', 129),

('Arjun Reddy',
 'A brilliant but self-destructive surgeon spirals after losing the love of his life.',
 2017, 187, 'Telugu', 'Vijay Deverakonda, Shalini Pandey', 'Sandeep Reddy Vanga',
 'A', 'Strong Language, Substance Use', 'Drama', 99),

('Ala Vaikunthapurramuloo',
 'A man raised in the wrong family discovers his true identity and fights for his place.',
 2020, 167, 'Telugu', 'Allu Arjun, Pooja Hegde', 'Trivikram Srinivas',
 'U', 'Mild Thematic Elements', 'Comedy', 94),

-- MALAYALAM
('Drishyam',
 'A cable operator uses his knowledge of films to protect his family from a murder charge.',
 2013, 160, 'Malayalam', 'Mohanlal, Meena', 'Jeethu Joseph',
 'U/A', 'Thematic Elements, Mild Violence', 'Thriller', 99),

('Premam',
 'A young man experiences three distinct phases of love across different stages of life.',
 2015, 140, 'Malayalam', 'Nivin Pauly, Sai Pallavi', 'Alphonse Puthren',
 'U', 'Mild Thematic Elements', 'Romance', 84),

('Kumbalangi Nights',
 'Four dysfunctional brothers in a coastal village learn to become a family.',
 2019, 135, 'Malayalam', 'Shane Nigam, Fahadh Faasil', 'Madhu C. Narayanan',
 'U/A', 'Thematic Elements', 'Drama', 89),

('Lucifer',
 'A mysterious man rises to power in Kerala politics following the death of a beloved leader.',
 2019, 170, 'Malayalam', 'Mohanlal, Vivek Oberoi', 'Prithviraj Sukumaran',
 'U/A', 'Violence, Thematic Elements', 'Action', 104),

('Manjadikuru',
 'A young boy spends a summer at his ancestral home and discovers family secrets.',
 2007, 110, 'Malayalam', 'Master Sai, Kavya Madhavan', 'Anjali Menon',
 'U', 'Mild Thematic Elements', 'Drama', 74);

-- Insert movies (skip if title already exists)
INSERT INTO dbo.Movies (
    Title, [Description], ReleaseYear, DurationMinutes, [Language],
    Rating, Cast, Director, ContentRating, ContentAdvisory,
    RentalCount, MyProperty, PosterPath, TrailerUrl
)
SELECT
    m.Title, m.[Description], m.ReleaseYear, m.DurationMinutes, m.[Language],
    0, m.CastCsv, m.Director, m.ContentRating, m.ContentAdvisory,
    0, 0,
    CONCAT('https://picsum.photos/seed/', LOWER(REPLACE(REPLACE(m.Title, ' ', '-'), ':', '')), '/500/750'),
    CONCAT('https://www.youtube.com/results?search_query=', REPLACE(m.Title, ' ', '+'), '+official+trailer')
FROM @IndianMovies m
WHERE NOT EXISTS (SELECT 1 FROM dbo.Movies ex WHERE ex.Title = m.Title);

-- Link genres
INSERT INTO dbo.GenreMovie (GenresId, MoviesId)
SELECT g.Id, mv.Id
FROM @IndianMovies m
INNER JOIN dbo.Genres g  ON g.Name  = m.GenreName
INNER JOIN dbo.Movies mv ON mv.Title = m.Title
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.GenreMovie gm WHERE gm.GenresId = g.Id AND gm.MoviesId = mv.Id
);

-- Add inventory (one entry per movie)
INSERT INTO dbo.Inventories (MovieId, RentalPrice, IsAvailable)
SELECT mv.Id, m.RentalPrice, 1
FROM @IndianMovies m
INNER JOIN dbo.Movies mv ON mv.Title = m.Title
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Inventories inv WHERE inv.MovieId = mv.Id
);

COMMIT TRANSACTION;-- Indian Movies Poster Update
UPDATE dbo.Movies SET PosterPath = 'https://image.tmdb.org/t/p/w500/1CoKNi3XVyijPCvy0usDbSWEXAg.jpg', LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/l0fNAHLOFReQJsxCOmGWvJDnimn.jpg' WHERE Title = 'Dangal';
UPDATE dbo.Movies SET PosterPath = 'https://image.tmdb.org/t/p/w500/66A9MqXOyVFCssoloscw79z8Tew.jpg', LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/8gT3UKtglLVpu0YfccwbmXZ5Eis.jpg' WHERE Title = '3 Idiots';
UPDATE dbo.Movies SET PosterPath = 'https://image.tmdb.org/t/p/w500/yNX9lFRAFeNLNRIXdqZK9gYrYKa.jpg', LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/76VwduxQMkVXOUktps85dE0lLeP.jpg' WHERE Title = 'Lagaan';
UPDATE dbo.Movies SET PosterPath = 'https://image.tmdb.org/t/p/w500/xAy208Znkingmfnb5ZbULwLyIwW.jpg', LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/eByyqLrrdYySwYjus5RVCgbCNOD.jpg' WHERE Title = 'Gangs of Wasseypur';
UPDATE dbo.Movies SET PosterPath = 'https://image.tmdb.org/t/p/w500/dy3K6hNvwE05siGgiLJcEiwgpdO.jpg', LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/ArvKQJv3nEpnBoVyjWDUT7TtJOL.jpg' WHERE Title = 'Andhadhun';
UPDATE dbo.Movies SET PosterPath = 'https://image.tmdb.org/t/p/w500/c6Cicaf2FFmfcInfsbPTxMLk5CS.jpg', LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/27dhih2lFlBl0oIUE9jFwIEBIMg.jpg' WHERE Title = 'Dil Chahta Hai';
UPDATE dbo.Movies SET PosterPath = 'https://image.tmdb.org/t/p/w500/774UV1aCURb4s4JfEFg3IEMu5Zj.jpg', LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/dkIX4dSMuVqjfrPGunBJUR7K3LQ.jpg' WHERE Title = 'Vikram';
UPDATE dbo.Movies SET PosterPath = 'https://image.tmdb.org/t/p/w500/icWUNL2GxDgWmI5PQN2RYA2CEbv.jpg', LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/hGBleDZsJGo0epp50ekddIz9cvN.jpg' WHERE Title = 'Vinnaithaandi Varuvaayaa';
UPDATE dbo.Movies SET PosterPath = 'https://image.tmdb.org/t/p/w500/nT556HbikCCLABe8xQ4zQ2QIb0O.jpg', LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/9QaebfA43SkffIy6sDFje5q0jdk.jpg' WHERE Title = 'Anbe Sivam';
UPDATE dbo.Movies SET PosterPath = 'https://image.tmdb.org/t/p/w500/mxvOvom5zKRp4WPURKrhjoatt4P.jpg', LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/seTFDq1iKyJ6YHr7HePbC20DMsy.jpg' WHERE Title = 'Kaithi';
UPDATE dbo.Movies SET PosterPath = 'https://image.tmdb.org/t/p/w500/nrVloCa2hCFOztRF1DZU2jnWIiQ.jpg', LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/qzu94AvnZ1az30sTUuibx2bXpfs.jpg' WHERE Title = '96';
UPDATE dbo.Movies SET PosterPath = 'https://image.tmdb.org/t/p/w500/9BAjt8nSSms62uOVYn1t3C3dVto.jpg', LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/e9ZEuHGHZ06AToHlfN1L7nejJ7W.jpg' WHERE Title = 'Baahubali: The Beginning';
UPDATE dbo.Movies SET PosterPath = 'https://image.tmdb.org/t/p/w500/21sC2assImQIYCEDA84Qh9d1RsK.jpg', LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/whNjsTOUVg2lZLCKgGhnACnmV8E.jpg' WHERE Title = 'Baahubali 2: The Conclusion';
UPDATE dbo.Movies SET PosterPath = 'https://image.tmdb.org/t/p/w500/kHubDgL59I5hCn7ccBYvU7bKY1r.jpg', LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/rhPJSfpy7s0x9jQPCGkqD2kvXAC.jpg' WHERE Title = 'Arjun Reddy';
UPDATE dbo.Movies SET PosterPath = 'https://image.tmdb.org/t/p/w500/goVGxWzvxs8oMNJ1Zc0QmfJlIzs.jpg', LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/5SDMPORslLXaYPx7S1kRqsBJYI3.jpg' WHERE Title = 'Ala Vaikunthapurramuloo';
UPDATE dbo.Movies SET PosterPath = 'https://image.tmdb.org/t/p/w500/7d8GLneJkF81q1POdK7VUrjWafX.jpg', LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/uqcM9eRqUyZCJCGDkLozP9TGAHn.jpg' WHERE Title = 'Drishyam';
UPDATE dbo.Movies SET PosterPath = 'https://image.tmdb.org/t/p/w500/wfMgsfDrtouYOM6MbrkHtU96Xij.jpg', LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/4vkgstaV6dWAJhGaBAH1MtgZsEK.jpg' WHERE Title = 'Premam';
UPDATE dbo.Movies SET PosterPath = 'https://image.tmdb.org/t/p/w500/lJ3RvIirE2C7gdBKvPRaoQ3iCo2.jpg', LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/8i8ml0LRdFT6LSaTMUG3gLzJfEq.jpg' WHERE Title = 'Kumbalangi Nights';
UPDATE dbo.Movies SET PosterPath = 'https://image.tmdb.org/t/p/w500/fXgY2RCzoIJPhPDoyKRjaaqjIZs.jpg', LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/8SFdD1gwynrsj3hCtINgC1EsNzB.jpg' WHERE Title = 'Lucifer';
UPDATE dbo.Movies SET PosterPath = 'NOT FOUND', LandscapePosterPath = 'NOT FOUND' WHERE Title = 'Manjadikuru';
PRINT '20 Indian movies inserted successfully.';
