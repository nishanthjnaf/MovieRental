/**
 * fetch_trailers_console.js
 * ─────────────────────────
 * Paste this entire script into your browser console.
 * Requires: backend running at http://localhost:5287
 *           TMDB API v3 key (free at themoviedb.org/settings/api)
 *
 * It fetches every movie & series from your DB, looks up the real
 * official trailer on TMDB, and prints ready-to-run SQL UPDATEs.
 */

const TMDB_KEY = 'YOUR_TMDB_API_KEY_HERE'; // ← replace with your key

// ─── helpers ────────────────────────────────────────────────────────────────
const sleep = ms => new Promise(r => setTimeout(r, ms));
const esc   = s  => String(s ?? '').replace(/'/g, "''");

async function tmdb(path) {
  const sep = path.includes('?') ? '&' : '?';
  const r = await fetch(`https://api.themoviedb.org/3${path}${sep}api_key=${TMDB_KEY}`);
  if (!r.ok) throw new Error(`TMDB ${r.status}: ${path}`);
  return r.json();
}

/** Search TMDB and return the best matching id */
async function findId(title, year, type /* 'movie'|'tv' */) {
  const yearParam = (type === 'movie' && year) ? `&year=${year}` : (year ? `&first_air_date_year=${year}` : '');
  const d = await tmdb(`/search/${type}?query=${encodeURIComponent(title)}&language=en-US${yearParam}`);
  if (d.results?.length) return d.results[0].id;

  // retry without year if nothing found
  if (year) {
    const d2 = await tmdb(`/search/${type}?query=${encodeURIComponent(title)}&language=en-US`);
    if (d2.results?.length) return d2.results[0].id;
  }
  return null;
}

/** Return the best YouTube trailer key for a TMDB id */
async function getTrailerKey(tmdbId, type) {
  const d = await tmdb(`/${type}/${tmdbId}/videos?language=en-US`);
  const vids = d.results ?? [];

  // priority order
  return (
    vids.find(v => v.site === 'YouTube' && v.type === 'Trailer' && v.official)?.key ||
    vids.find(v => v.site === 'YouTube' && v.type === 'Trailer')?.key              ||
    vids.find(v => v.site === 'YouTube' && v.type === 'Teaser'  && v.official)?.key ||
    vids.find(v => v.site === 'YouTube' && v.type === 'Teaser')?.key               ||
    vids.find(v => v.site === 'YouTube')?.key                                       ||
    null
  );
}

// ─── main ────────────────────────────────────────────────────────────────────
(async () => {
  if (TMDB_KEY === 'YOUR_TMDB_API_KEY_HERE') {
    console.error('❌  Set TMDB_KEY before running.');
    return;
  }

  // 1. load from local API
  console.log('📡 Loading movies & series from local API...');
  let movies = [], series = [];

  try {
    movies = await (await fetch('http://localhost:5287/api/v1/Movie')).json();
    console.log(`   ✅ ${movies.length} movies`);
  } catch(e) { console.error('   ❌ movies:', e.message); }

  try {
    series = await (await fetch('http://localhost:5287/api/Series')).json();
    console.log(`   ✅ ${series.length} series`);
  } catch(e) { console.error('   ❌ series:', e.message); }

  const movieSql = [], seriesSql = [], missing = [];

  // 2. process movies
  console.log('\n🎬 Fetching movie trailers...');
  for (const m of movies) {
    const title = m.title ?? m.Title;
    const year  = m.releaseYear ?? m.ReleaseYear;
    const id    = m.id ?? m.Id;
    try {
      const tmdbId = await findId(title, year, 'movie');
      await sleep(260);
      if (!tmdbId) { console.warn(`  ⚠️  no TMDB match: "${title}"`); missing.push(title); continue; }

      const key = await getTrailerKey(tmdbId, 'movie');
      await sleep(260);
      if (!key)   { console.warn(`  ⚠️  no trailer:    "${title}" (tmdb:${tmdbId})`); missing.push(title); continue; }

      movieSql.push(`UPDATE dbo.Movies SET TrailerUrl = 'https://www.youtube.com/watch?v=${esc(key)}' WHERE Id = ${id}; -- ${esc(title)}`);
      console.log(`  ✅ "${title}" → ${key}`);
    } catch(e) {
      console.error(`  ❌ "${title}":`, e.message);
      missing.push(title);
      await sleep(500);
    }
  }

  // 3. process series
  console.log('\n📺 Fetching series trailers...');
  for (const s of series) {
    const title = s.title ?? s.Title;
    const year  = s.releaseYear ?? s.ReleaseYear ?? null;
    const id    = s.id ?? s.Id;
    try {
      const tmdbId = await findId(title, year, 'tv');
      await sleep(260);
      if (!tmdbId) { console.warn(`  ⚠️  no TMDB match: "${title}"`); missing.push(title); continue; }

      const key = await getTrailerKey(tmdbId, 'tv');
      await sleep(260);
      if (!key)   { console.warn(`  ⚠️  no trailer:    "${title}" (tmdb:${tmdbId})`); missing.push(title); continue; }

      seriesSql.push(`UPDATE dbo.Series SET TrailerUrl = 'https://www.youtube.com/watch?v=${esc(key)}' WHERE Id = ${id}; -- ${esc(title)}`);
      console.log(`  ✅ "${title}" → ${key}`);
    } catch(e) {
      console.error(`  ❌ "${title}":`, e.message);
      missing.push(title);
      await sleep(500);
    }
  }

  // 4. print SQL
  const lines = [
    '-- ── Movie Trailers ──────────────────────────────────────────────────',
    ...movieSql,
    '',
    '-- ── Series Trailers ─────────────────────────────────────────────────',
    ...seriesSql,
  ];

  console.log('\n\n========== COPY SQL BELOW ==========\n');
  console.log(lines.join('\n'));
  console.log('\n========== END OF SQL ==========\n');

  if (missing.length) {
    console.warn(`\n⚠️  ${missing.length} item(s) had no trailer on TMDB:`);
    missing.forEach(t => console.warn('   -', t));
  }

  console.log(`\n✅ Done — ${movieSql.length + seriesSql.length} trailers found, ${missing.length} missing.`);
  console.log('Run the SQL above in SSMS against your DBMovie database.');
})();
