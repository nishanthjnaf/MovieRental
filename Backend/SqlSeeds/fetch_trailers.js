/**
 * fetch_trailers.js
 * -----------------
 * Paste this entire script into your browser console.
 * It will:
 *   1. Fetch all movies from your local API
 *   2. Look up each movie on TMDB by title + year
 *   3. Fetch the official YouTube trailer for each
 *   4. Print a ready-to-run SQL UPDATE script to the console
 *
 * Prerequisites:
 *   - Your backend must be running at http://localhost:5287
 *   - Replace TMDB_API_KEY below with your TMDB API v3 key
 *     (get one free at https://www.themoviedb.org/settings/api)
 */

const TMDB_API_KEY = 'YOUR_TMDB_API_KEY_HERE'; // <-- replace this
const API_BASE     = 'http://localhost:5287/api/v1/Movie';
const TMDB_BASE    = 'https://api.themoviedb.org/3';

// Delay helper to avoid hitting TMDB rate limits (40 req/10s)
const delay = ms => new Promise(r => setTimeout(r, ms));

async function fetchAllMovies() {
  const res = await fetch(API_BASE);
  if (!res.ok) throw new Error(`API error: ${res.status}`);
  return res.json();
}

async function findTmdbId(title, year) {
  const url = `${TMDB_BASE}/search/movie?api_key=${TMDB_API_KEY}&query=${encodeURIComponent(title)}&year=${year}&language=en-US`;
  const res = await fetch(url);
  const data = await res.json();
  return data.results?.[0]?.id ?? null;
}

async function getTrailerKey(tmdbId) {
  const url = `${TMDB_BASE}/movie/${tmdbId}/videos?api_key=${TMDB_API_KEY}&language=en-US`;
  const res = await fetch(url);
  const data = await res.json();
  const videos = data.results || [];

  // Priority: official YouTube trailer → any YouTube trailer → teaser
  const pick =
    videos.find(v => v.site === 'YouTube' && v.type === 'Trailer' && v.official) ||
    videos.find(v => v.site === 'YouTube' && v.type === 'Trailer') ||
    videos.find(v => v.site === 'YouTube' && v.type === 'Teaser') ||
    videos.find(v => v.site === 'YouTube');

  return pick?.key ?? null;
}

function escapeSql(str) {
  return str.replace(/'/g, "''");
}

(async () => {
  if (TMDB_API_KEY === 'YOUR_TMDB_API_KEY_HERE') {
    console.error('❌  Set your TMDB_API_KEY before running this script.');
    return;
  }

  console.log('📡  Fetching movies from local API...');
  let movies;
  try {
    movies = await fetchAllMovies();
  } catch (e) {
    console.error('❌  Could not reach local API:', e.message);
    return;
  }
  console.log(`✅  Found ${movies.length} movies. Fetching trailers from TMDB...`);

  const updates = [];
  const failed  = [];

  for (let i = 0; i < movies.length; i++) {
    const m = movies[i];
    const title = m.title || m.Title;
    const year  = m.releaseYear || m.ReleaseYear;
    const id    = m.id || m.Id;

    process?.stdout?.write?.(`  [${i+1}/${movies.length}] ${title}...`);

    try {
      const tmdbId = await findTmdbId(title, year);
      if (!tmdbId) {
        console.warn(`  ⚠️  [${i+1}] No TMDB match: "${title}" (${year})`);
        failed.push({ title, year });
        await delay(260);
        continue;
      }

      await delay(130); // stay under rate limit

      const key = await getTrailerKey(tmdbId);
      if (!key) {
        console.warn(`  ⚠️  [${i+1}] No trailer found: "${title}" (TMDB ${tmdbId})`);
        failed.push({ title, year });
        await delay(130);
        continue;
      }

      const youtubeUrl = `https://www.youtube.com/watch?v=${key}`;
      updates.push({ id, title, youtubeUrl });
      console.log(`  ✅  [${i+1}] "${title}" → ${youtubeUrl}`);
    } catch (e) {
      console.error(`  ❌  [${i+1}] Error for "${title}":`, e.message);
      failed.push({ title, year });
    }

    await delay(260); // ~3-4 req/s, well within TMDB's 40/10s limit
  }

  // ── Output SQL ──────────────────────────────────────────────────────────────
  console.log('\n\n========== COPY THE SQL BELOW ==========\n');

  if (updates.length === 0) {
    console.warn('No trailers found — nothing to update.');
    return;
  }

  const sql = updates.map(u =>
    `UPDATE dbo.Movies SET TrailerUrl = '${escapeSql(u.youtubeUrl)}' WHERE Id = ${u.id}; -- ${escapeSql(u.title)}`
  ).join('\n');

  console.log(sql);
  console.log('\n========== END OF SQL ==========\n');

  if (failed.length > 0) {
    console.warn(`\n⚠️  ${failed.length} movie(s) had no trailer found:`);
    failed.forEach(f => console.warn(`   - "${f.title}" (${f.year})`));
  }

  console.log(`\n✅  Done. ${updates.length} trailers found, ${failed.length} missing.`);
  console.log('Run the SQL above in SSMS or sqlcmd against your DBMovie database.');
})();
