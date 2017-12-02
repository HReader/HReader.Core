INSERT INTO Tag(name)
VALUES($value);

SELECT rowid
FROM Tag
WHERE name = $value;