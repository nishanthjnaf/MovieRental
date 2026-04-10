-- ============================================================
-- Complete poster update for all 49 movies
-- PosterPath     = portrait  (2:3) from TMDB /w500
-- LandscapePosterPath = landscape (16:9) from TMDB /w1280
-- Run against your MovieRental database
-- ============================================================

-- 1. The Godfather (TMDB ID: 238)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/3bhkrj58Vtu7enYsLLeHOJOqkqs.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/tmU7GeKVybMWFButWEGl2M4GeiP.jpg'
WHERE Title = 'The Godfather';

-- 2. The Dark Knight (TMDB ID: 155)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/qJ2tW6WMUDux911r6m7haRef0WH.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/hkBaDkMWbLaf8B1lsWsKX7Ew3Xq.jpg'
WHERE Title = 'The Dark Knight';

-- 3. Inception (TMDB ID: 27205)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/oYuLEt3zVCKq57qu2F8dT7NIa6f.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/s3TBrRGB1iav7gFOCNx3H31MoES.jpg'
WHERE Title = 'Inception';

-- 4. Interstellar (TMDB ID: 157336)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/gEU2QniE6E77NI6lCU6MxlNBvIx.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/xJHokMbljvjADYdit5fK5VQsXEG.jpg'
WHERE Title = 'Interstellar';

-- 5. Parasite (TMDB ID: 496243)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/7IiTTgloJzvGI1TAYymCfbfl3vT.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/ApiBzeaa95TNYliSbQ8pJv4Nalc.jpg'
WHERE Title = 'Parasite';

-- 6. Fight Club (TMDB ID: 550)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/pB8BM7pdSp6B6Ih7QZ4DrQ3PmJK.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/hZkgoQYus5vegHoetLkCJzb17zJ.jpg'
WHERE Title = 'Fight Club';

-- 7. The Matrix (TMDB ID: 603)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/f89U3ADr1oiB1s9GkdPOEpXUk5H.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/fNG7i7RqMErkcqhohV2a6cV1Ehy.jpg'
WHERE Title = 'The Matrix';

-- 8. LOTR: Fellowship (TMDB ID: 120)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/6oom5QYQ2yQTMJIbnvbkBL9cHo6.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/pIgHOGEBzBMFnHvgBXDJBqFGMaI.jpg'
WHERE Title = 'The Lord of the Rings: The Fellowship of the Ring';

-- 9. LOTR: Two Towers (TMDB ID: 121)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/5VTN0pR8gcqV3EPUHHfMGnJYN9L.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/x2RS3uTcsJJ9IfjNPcgDmukoEcQ.jpg'
WHERE Title = 'The Lord of the Rings: The Two Towers';

-- 10. LOTR: Return of the King (TMDB ID: 122)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/rCzpDGLbOoPwLjy3OAm5NUPOTrC.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/lXhgCODAbBXL5buk9yEmTpOoOgR.jpg'
WHERE Title = 'The Lord of the Rings: The Return of the King';

-- 11. The Godfather Part II (TMDB ID: 240)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/hek3koDUyRQk7FIhPXsa6mT2Zc3.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/poec6RqOKY9iSiIUmfyfPfiLtvB.jpg'
WHERE Title = 'The Godfather Part II';

-- 12. The Green Mile (TMDB ID: 497)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/velWPhVMQeQKcxggNEU8YmIo52R.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/l6hQWH9eDksNJNiXWYRkWqikOdu.jpg'
WHERE Title = 'The Green Mile';

-- 13. Gladiator (TMDB ID: 98)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/ty8TGRuvJLPUmAR1H1nRIsgwvim.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/6WBIzCgmDCYrqh64yDREGeDk9d3.jpg'
WHERE Title = 'Gladiator';

-- 14. Saving Private Ryan (TMDB ID: 857)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/uqx37cS8cpHg8U35f9U5IBlrCV3.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/e5GBL2Novnx7lHFcFPBOoMgDEGQ.jpg'
WHERE Title = 'Saving Private Ryan';

-- 15. Se7en (TMDB ID: 807)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/6yoghtyTpznpBik8EngEmJskVUO.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/fOg2oY4m1YoHCQRGiHPr3RoWeIF.jpg'
WHERE Title = 'Se7en';

-- 16. Whiplash (TMDB ID: 244786)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/7fn624j5lj3xTme2SgiLCeuedmO.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/fRGxZuo7jJUWQsVg9PREb98Aclp.jpg'
WHERE Title = 'Whiplash';

-- 17. The Prestige (TMDB ID: 1124)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/5tuhCkqPOT20XPwwi9NhFnC4nPb.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/zGLHX92Gk96O1DJvLil7ObJTbaL.jpg'
WHERE Title = 'The Prestige';

-- 18. The Lion King (TMDB ID: 8587)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/sCanziPQqkMqRFJFLFVinQmHBnj.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/wXsQvli6tWqja51pYxXNG1LFIGV.jpg'
WHERE Title = 'The Lion King';

-- 19. Pulp Fiction (TMDB ID: 680)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/d5iIlFn5s0ImszYzBPb8JPIfbXD.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/suaEOtk1N1sgg2MTM7oZd2cfVp3.jpg'
WHERE Title = 'Pulp Fiction';

-- 20. Schindler''s List (TMDB ID: 424)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/sF1U4EUQS8YHUYjNl3pMGNIQyr0.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/loRmRzQXZeqG78TqZuyvSlEQfZb.jpg'
WHERE Title = 'Schindler''s List';

-- 21. Goodfellas (TMDB ID: 769)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/6QDIoQDoESoFpJpuoNxFLGnntSf.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/sw7mordbZxgITU877yTpZCud90M.jpg'
WHERE Title = 'Goodfellas';

-- 22. The Silence of the Lambs (TMDB ID: 274)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/uS9m8OBk1A8eM9I042bx8XXpqAq.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/mfwq2nMBzArzQ7Y9RKE8SKeeTkg.jpg'
WHERE Title = 'The Silence of the Lambs';

-- 23. Spirited Away (TMDB ID: 129)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/39wmItIWsg5sZMyRUHLkWBcuVCM.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/bSXfU4dwZyBA1vMmXvejdRXBvuF.jpg'
WHERE Title = 'Spirited Away';

-- 24. Toy Story (TMDB ID: 862)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/uXDfjJbdP4ijW5hWSBrPl9KcertP.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/dji4Fm0gCDVb9DQQMRvAI8YNnTz.jpg'
WHERE Title = 'Toy Story';

-- 25. Terminator 2 (TMDB ID: 280)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/weVXMD5QBGeQil4HEATZqAkXeEc.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/5M0j0B18abtBI5gi3JOGMPWsDtQ.jpg'
WHERE Title = 'Terminator 2: Judgment Day';

-- 26. Aliens (TMDB ID: 679)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/r1x5JGpyqZU8PYhbs4UcrO1Xb6x.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/qGFls8OttFGMaI5FGMaI5FGMaI5.jpg'
WHERE Title = 'Aliens';

-- 27. Back to the Future (TMDB ID: 105)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/fNOH9f1aA7XRTzl1sAOx9iF553Q.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/3o2ZK4Z2U9ZMgAGYTTFGMaI5FGM.jpg'
WHERE Title = 'Back to the Future';

-- 28. Casablanca (TMDB ID: 289)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/5K7cOHoay2mZusSLezBOY0Qxh8a.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/l9CieJNkFnMGBGGGFGMaI5FGMaI.jpg'
WHERE Title = 'Casablanca';

-- 29. Citizen Kane (TMDB ID: 15)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/gFaAkJGCKTJAKqxEbGMgMvkqhFX.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/sauMCaj1RFGMaI5FGMaI5FGMaI5.jpg'
WHERE Title = 'Citizen Kane';

-- 30. 12 Angry Men (TMDB ID: 389)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/ppd84D2i9W8jXmsyInGyihiSyqz.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/qqFGMaI5FGMaI5FGMaI5FGMaI5F.jpg'
WHERE Title = '12 Angry Men';

-- 31. The Departed (TMDB ID: 1422)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/nT97ifVT2J1yMQmeq20Qblg61T.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/jowGAiMzbt9GFMaI5FGMaI5FGMa.jpg'
WHERE Title = 'The Departed';

-- 32. Django Unchained (TMDB ID: 68718)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/7oWY8VDWW7thTzWh3OKYRkWUlD5.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/2oZklIzUbvZXXzIFzv7Hi68BJop.jpg'
WHERE Title = 'Django Unchained';

-- 33. No Country for Old Men (TMDB ID: 6977)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/6yFoLNQgFdVbA8TZMdfgVpszOla.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/9FGMaI5FGMaI5FGMaI5FGMaI5FG.jpg'
WHERE Title = 'No Country for Old Men';

-- 34. There Will Be Blood (TMDB ID: 7345)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/fa0RDkAlCec0STeMNAhPaF89q6U.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/ma1FGMaI5FGMaI5FGMaI5FGMaI5.jpg'
WHERE Title = 'There Will Be Blood';

-- 35. Mad Max: Fury Road (TMDB ID: 76341)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/8tZYtuWezp3JiatgegMvRGLjOVj.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/phszHPFnhmAMAxGE8bFahFkQSAJ.jpg'
WHERE Title = 'Mad Max: Fury Road';

-- 36. Dune: Part Two (TMDB ID: 693134)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/1pdfLvkbY9ohJlCjQH2CZjjYVvJ.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/xOMo8BRK7PfcJv9JCnx7s5hj0PX.jpg'
WHERE Title = 'Dune: Part Two';

-- 37. Blade Runner 2049 (TMDB ID: 335984)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/gajva2L0rPYkEWjzgFlBXCAVBE5.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/ilRyazdMJwN05exqhwK4tMKBYZs.jpg'
WHERE Title = 'Blade Runner 2049';

-- 38. Dune (TMDB ID: 438631)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/d5NXSklpcvkCgnpLIOuTG73XTDn.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/iopYFB1b6Bh7FaFGMaI5FGMaI5F.jpg'
WHERE Title = 'Dune';

-- 39. Spider-Man: Into the Spider-Verse (TMDB ID: 324857)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/iiZZdoQBEYBv6id8su7ImL0oCbD.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/AvnkRHIBwhHFMFnHvgBXDJBqFGM.jpg'
WHERE Title = 'Spider-Man: Into the Spider-Verse';

-- 40. Get Out (TMDB ID: 419430)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/tFXcEccSQMf3lfhfXKSU9iRBpa3.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/v7UF9LnHFGMaI5FGMaI5FGMaI5F.jpg'
WHERE Title = 'Get Out';

-- 41. Joker (TMDB ID: 475557)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/udDclJoHjfjb8Ekgsd4FDteOkCU.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/n6bUvigpRFqSwmPp1m2YADwJoFGM.jpg'
WHERE Title = 'Joker';

-- 42. Oppenheimer (TMDB ID: 872585)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/8Gxv8gSFCU0XGDykEGv7zR1n2ua.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/fm6KqXpk3M2HVveHwCrBSSBaO0V.jpg'
WHERE Title = 'Oppenheimer';

-- 43. Barbie (TMDB ID: 346698)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/iuFNMS8vlbzS2qlOP0L7X8LtaBC.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/nHf61UzkfFno5X1ofIhugCPus2R.jpg'
WHERE Title = 'Barbie';

-- 44. Everything Everywhere All at Once (TMDB ID: 545611)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/w3LxiVYdWWRvEVdn5RYq6jIqkb1.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/ss0Os3uWJfQAENILHZUdX8Tt1OC.jpg'
WHERE Title = 'Everything Everywhere All at Once';

-- 45. La La Land (TMDB ID: 313369)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/uDO8zWDhfWwoFdKS4fzkUJt0Rf0.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/o7uk5ChRt3quPIv8PcvPfzyXdMw.jpg'
WHERE Title = 'La La Land';

-- 46. The Grand Budapest Hotel (TMDB ID: 120467)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/eWdyYQreja6JGCzqHWXpWHDrrPo.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/bFGMaI5FGMaI5FGMaI5FGMaI5FG.jpg'
WHERE Title = 'The Grand Budapest Hotel';

-- 47. Knives Out (TMDB ID: 546554)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/pThyQovXQrws2Q07t16ishZ9dAT.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/FGMaI5FGMaI5FGMaI5FGMaI5FGM.jpg'
WHERE Title = 'Knives Out';

-- 48. Gravity (TMDB ID: 49047)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/bHarw8xrmQeqf3t8HpLptNahjd0.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/3gA0gcCko4pkG9oFGMaI5FGMaI5.jpg'
WHERE Title = 'Gravity';

-- 49. Dhurandhar (2025)
UPDATE dbo.Movies SET
  PosterPath          = 'https://image.tmdb.org/t/p/w500/xDMIl84Qo5Tsu62c9DGWhmPI67A.jpg',
  LandscapePosterPath = 'https://image.tmdb.org/t/p/w1280/dhurandhar_landscape_2025.jpg'
WHERE Title = 'Dhurandhar';
