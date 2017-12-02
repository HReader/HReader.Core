SELECT
  t.name as tag
FROM
  EntryFitsTag e
  INNER JOIN Tag t ON e.tag = t.rowid
WHERE
  e.entry = $entry;