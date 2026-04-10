// ============================================================
// Run this in browser console from https://www.themoviedb.org
// Replace YOUR_TMDB_API_KEY with your actual key
// ============================================================

const API_KEY = 'YOUR_TMDB_API_KEY';
const base = 'https://image.tmdb.org/t/p';

const movies = [
  { title: 'Dangal',                          year: 2016, lang: 'hi' },
  { title: '3 Idiots',                         year: 2009, lang: 'hi' },
  { title: 'Lagaan',                           year: 2001, lang: 'hi' },
  { title: 'Gangs of Wasseypur',               year: 2012, lang: 'hi' },
  { title: 'Andhadhun',                        year: 2018, lang: 'hi' },
  { title: 'Dil Chahta Hai',                   year: 2001, lang: 'hi' },
  { title: 'Vikram',                           year: 2022, lang: 'ta' },
  { title: 'Vinnaithaandi Varuvaayaa',         year: 2010, lang: 'ta' },
  { title: 'Anbe Sivam',                       year: 2003, lang: 'ta' },
  { title: 'Kaithi',                           year: 2019, lang: 'ta' },
  { title: '96',                               year: 2018, lang: 'ta' },
  { title: 'Baahubali: The Beginning',         year: 2015, lang: 'te' },
  { title: 'Baahubali 2: The Conclusion',      year: 2017, lang: 'te' },
  { title: 'Arjun Reddy',                      year: 2017, lang: 'te' },
  { title: 'Ala Vaikunthapurramuloo',          year: 2020, lang: 'te' },
  { title: 'Drishyam',                         year: 2013, lang: 'ml' },
  { title: 'Premam',                           year: 2015, lang: 'ml' },
  { title: 'Kumbalangi Nights',                year: 2019, lang: 'ml' },
  { title: 'Lucifer',                          year: 2019, lang: 'ml' },
  { title: 'Manjadikuru',                      year: 2007, lang: 'ml' },
];

const results = [];

for (const m of movies) {
  try {
    const res = await fetch(
      `https://api.themoviedb.org/3/search/movie?api_key=${API_KEY}&query=${encodeURIComponent(m.title)}&year=${m.year}&language=en-US`
    );
    const data = await res.json();
    const hit = data.results?.[0];
    results.push({
      title:     m.title,
      poster:    hit?.poster_path    ? `${base}/w500${hit.poster_path}`    : 'NOT FOUND',
      landscape: hit?.backdrop_path  ? `${base}/w1280${hit.backdrop_path}` : 'NOT FOUND'
    });
  } catch (e) {
    results.push({ title: m.title, poster: 'ERROR', landscape: 'ERROR' });
  }
}

let sql = '-- Indian Movies Poster Update\n';
for (const r of results) {
  const t = r.title.replace(/'/g, "''");
  sql += `UPDATE dbo.Movies SET PosterPath = '${r.poster}', LandscapePosterPath = '${r.landscape}' WHERE Title = '${t}';\n`;
}

console.log(sql);
console.log('\n✅ Done! Copy the SQL above and run in SSMS.');
