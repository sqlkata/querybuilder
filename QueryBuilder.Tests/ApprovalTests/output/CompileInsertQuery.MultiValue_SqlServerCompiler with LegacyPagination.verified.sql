-------- ORIGINAL -----------
INSERT INTO [X] ([a]) 
VALUES (1), 
(2)

----------- RAW -------------
INSERT INTO [X] ([a]) 
VALUES (?), 
(?)

--------PARAMETRIZED --------
INSERT INTO [X] ([a]) 
VALUES (@p0), 
(@p1)