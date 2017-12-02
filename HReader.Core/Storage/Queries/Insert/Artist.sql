INSERT INTO Artist(name)
VALUES ($value);

SELECT rowid
FROM Artist
WHERE name = $value;