-------- ORIGINAL -----------
INSERT INTO "X" ("a") 
VALUES (3);
SELECT lastval() AS id

----------- RAW -------------
INSERT INTO "X" ("a") 
VALUES (?);
SELECT lastval() AS id

--------PARAMETRIZED --------
INSERT INTO "X" ("a") 
VALUES (@p0);
SELECT lastval() AS id