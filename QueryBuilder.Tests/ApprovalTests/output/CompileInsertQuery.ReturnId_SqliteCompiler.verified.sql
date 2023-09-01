-------- ORIGINAL -----------
INSERT INTO "X" ("a") 
VALUES (3);select last_insert_rowid() as id

----------- RAW -------------
INSERT INTO "X" ("a") 
VALUES (?);select last_insert_rowid() as id

--------PARAMETRIZED --------
INSERT INTO "X" ("a") 
VALUES (@p0);select last_insert_rowid() as id