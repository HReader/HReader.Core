SELECT
  a.name as artist
FROM
  EntryDrawnByArtist e
  INNER JOIN Artist a ON e.artist = a.rowid
WHERE
  e.entry = $entry;