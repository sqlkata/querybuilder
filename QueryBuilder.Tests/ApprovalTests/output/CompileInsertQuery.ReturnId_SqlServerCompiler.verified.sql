-------- ORIGINAL -----------
INSERT INTO [X] ([a]) 
VALUES (3);
SELECT scope_identity() as Id

----------- RAW -------------
INSERT INTO [X] ([a]) 
VALUES (?);
SELECT scope_identity() as Id

--------PARAMETRIZED --------
INSERT INTO [X] ([a]) 
VALUES (@p0);
SELECT scope_identity() as Id