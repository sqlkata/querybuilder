-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE EXISTS (
SELECT 1 
FROM "Y" 
WHERE "A" = 4)

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE EXISTS (
SELECT 1 
FROM "Y" 
WHERE "A" = ?)

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE EXISTS (
SELECT 1 
FROM "Y" 
WHERE "A" = @p0)