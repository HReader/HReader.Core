INSERT INTO Entry (title, lang, kind, cover)
VALUES ($title, $lang, $kind, $cover);
SELECT last_insert_rowid();