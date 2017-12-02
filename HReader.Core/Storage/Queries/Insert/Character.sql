INSERT INTO Character(name)
VALUES($value);

SELECT rowid
FROM Character
WHERE name = $value;