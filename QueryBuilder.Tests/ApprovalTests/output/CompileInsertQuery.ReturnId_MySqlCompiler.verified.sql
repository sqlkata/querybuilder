-------- ORIGINAL -----------
INSERT INTO `X` (`a`) 
VALUES (3);
SELECT last_insert_id() as Id

----------- RAW -------------
INSERT INTO `X` (`a`) 
VALUES (?);
SELECT last_insert_id() as Id

--------PARAMETRIZED --------
INSERT INTO `X` (`a`) 
VALUES (@p0);
SELECT last_insert_id() as Id