﻿CREATE TABLE IF NOT EXISTS Artist (
  rowid INTEGER PRIMARY KEY,
  name TEXT UNIQUE ON CONFLICT IGNORE NOT NULL
);

CREATE TABLE IF NOT EXISTS Series (
  rowid INTEGER PRIMARY KEY,
  name TEXT UNIQUE ON CONFLICT IGNORE NOT NULL
);

CREATE TABLE IF NOT EXISTS Character (
  rowid INTEGER PRIMARY KEY,
  name TEXT UNIQUE NOT NULL
);

CREATE TABLE IF NOT EXISTS Tag (
  rowid INTEGER PRIMARY KEY,
  name TEXT UNIQUE ON CONFLICT IGNORE NOT NULL
);

CREATE TABLE IF NOT EXISTS Entry (
  id INTEGER PRIMARY KEY,
  title TEXT NOT NULL,
  lang TEXT NOT NULL,
  kind TEXT NOT NULL,
  cover TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Page (
  uri TEXT PRIMARY KEY,
  idx INTEGER NOT NULL,
  entry INTEGER,
  FOREIGN KEY(entry) REFERENCES Entry
) WITHOUT ROWID;

CREATE TABLE IF NOT EXISTS EntryDrawnByArtist (
  entry INTEGER,
  artist INTEGER,
  PRIMARY KEY(entry, artist),
  FOREIGN KEY(entry) REFERENCES Entry,
  FOREIGN KEY(artist) REFERENCES Artist
) WITHOUT ROWID;

CREATE TABLE IF NOT EXISTS EntryContainsSeries (
  entry INTEGER,
  series INTEGER,
  PRIMARY KEY(entry, series),
  FOREIGN KEY(entry) REFERENCES Entry,
  FOREIGN KEY(series) REFERENCES Series
) WITHOUT ROWID;

CREATE TABLE IF NOT EXISTS EntryContainsCharacter (
  entry INTEGER,
  character INTEGER,
  PRIMARY KEY(entry, character),
  FOREIGN KEY(entry) REFERENCES Entry,
  FOREIGN KEY(character) REFERENCES Character
) WITHOUT ROWID;

CREATE TABLE IF NOT EXISTS EntryFitsTag (
  entry INTEGER,
  tag INTEGER,
  PRIMARY KEY(entry, tag),
  FOREIGN KEY(entry) REFERENCES Entry,
  FOREIGN KEY(tag) REFERENCES Tag
) WITHOUT ROWID;