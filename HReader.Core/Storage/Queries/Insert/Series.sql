INSERT INTO Series(name)
VALUES ($value);

SELECT rowid
FROM Series
WHERE name = $value;