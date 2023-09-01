-------- ORIGINAL -----------
WITH [q] AS (
SELECT [a], [b], [c] 
FROM (
VALUES (1, 'k', NULL), 
(2, NULL, 'j')) 
AS tbl ([a], [b], [c]))

SELECT * 
FROM [X]

----------- RAW -------------
WITH [q] AS (
SELECT [a], [b], [c] 
FROM (
VALUES (?, ?, ?), 
(?, ?, ?)) 
AS tbl ([a], [b], [c]))

SELECT * 
FROM [X]

--------PARAMETRIZED --------
WITH [q] AS (
SELECT [a], [b], [c] 
FROM (
VALUES (@p0, @p1, @p2), 
(@p3, @p4, @p5)) 
AS tbl ([a], [b], [c]))

SELECT * 
FROM [X]