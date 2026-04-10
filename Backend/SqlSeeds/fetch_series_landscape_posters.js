// ============================================================
// Run from https://www.themoviedb.org console
// Gets BOTH portrait + landscape posters for all 16 series
// ============================================================

const API_KEY = 'YOUR_TMDB_API_KEY';
const base = 'https://image.tmdb.org/t/p';

const series = [
  { title: 'Breaking Bad',     year: 2008 },
  { title: 'Stranger Things',  year: 2016 },
  { title: 'The Crown',        year: 2016 },
  { title: 'Chernobyl',        year: 2019 },
  { title: 'Narcos',           year: 2015 },
  { title: 'The Boys',         year: 2019 },
  { title: 'Game of Thrones',  year: 2011 },
  { title: 'Dark',             year: 2017 },
  { title: 'Peaky Blinders',   year: 2013 },
  { title: 'Money Heist',      year: 2017 },
  { title: 'Mindhunter',       year: 2017 },
  { title: 'Squid Game',       year: 2021 },
  { title: 'The Wire',         year: 2002 },
  { title: 'Succession',       year: 2018 },
  { title: 'Ozark',            year: 2017 },
  { title: 'Scam 1992',        year: 2020 },
];

const results = [];

for (const s of series) {
  try {
    const res = await fetch(
      `https://api.themoviedb.org/3/search/tv?api_key=${API_KEY}&query=${encodeURIComponent(s.title)}&first_air_date_year=${s.year}&language=en-US`
    );
    const data = await res.json();
    const hit = data.results?.[0];
    results.push({
      title:     s.title,
      poster:    hit?.poster_path   ? `${base}/w500${hit.poster_path}`    : 'NOT FOUND',
      landscape: hit?.backdrop_path ? `${base}/w1280${hit.backdrop_path}` : 'NOT FOUND'
    });
  } catch (e) {
    results.push({ title: s.title, poster: 'ERROR', landscape: 'ERROR' });
  }
}

let sql = '-- Series: Portrait + Landscape poster update\n';
for (const r of results) {
  const t = r.title.replace(/'/g, "''");
  sql += `UPDATE dbo.Series SET PosterPath = '${r.poster}', LandscapePosterPath = '${r.landscape}' WHERE Title = '${t}';\n`;
}

console.log(sql);
console.log('\n✅ Done! Copy the SQL above and run in SSMS.');
