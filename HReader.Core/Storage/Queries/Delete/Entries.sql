DELETE FROM EntryFitsTag
WHERE entry IN ({0});

DELETE FROM EntryContainsCharacter
WHERE entry IN ({0});

DELETE FROM EntryContainsSeries
WHERE entry IN ({0});

DELETE FROM EntryDrawnByArtist
WHERE entry IN ({0});