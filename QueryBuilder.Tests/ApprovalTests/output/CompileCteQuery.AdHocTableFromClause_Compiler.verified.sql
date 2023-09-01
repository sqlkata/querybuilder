-------- ORIGINAL -----------
WITH "q" AS (
SELECT 1 AS "a", 'k' AS "b", NULL AS "c" 
UNION ALL 
SELECT 2 AS "a", NULL AS "b", 'j' AS "c")

SELECT * 
FROM "X"

----------- RAW -------------
WITH "q" AS (
SELECT ? AS "a", ? AS "b", ? AS "c" 
UNION ALL 
SELECT ? AS "a", ? AS "b", ? AS "c")

SELECT * 
FROM "X"

--------PARAMETRIZED --------
WITH "q" AS (
SELECT @p0 AS "a", @p1 AS "b", @p2 AS "c" 
UNION ALL 
SELECT @p3 AS "a", @p4 AS "b", @p5 AS "c")

SELECT * 
FROM "X"