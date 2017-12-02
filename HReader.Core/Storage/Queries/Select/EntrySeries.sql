SELECT
  s.name as series
FROM
  EntryContainsSeries e
  INNER JOIN Series s ON e.series = s.rowid
WHERE
  e.entry = $entry;