SELECT
  c.name as character
FROM
  EntryContainsCharacter e
  INNER JOIN Character c ON e.character = c.rowid
WHERE
  e.entry = $entry;