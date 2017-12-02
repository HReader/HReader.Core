DELETE FROM EntryFitsTag
WHERE entry = $entry;

DELETE FROM EntryContainsCharacter
WHERE entry = $entry;

DELETE FROM EntryContainsSeries
WHERE entry = $entry;

DELETE FROM EntryDrawnByArtist
WHERE entry = $entry;