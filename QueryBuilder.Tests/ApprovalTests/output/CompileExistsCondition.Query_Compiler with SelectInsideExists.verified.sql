-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE EXISTS (
SELECT * 
FROM "Y" 
WHERE "a" = 4)

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE EXISTS (
SELECT * 
FROM "Y" 
WHERE "a" = ?)

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE EXISTS (
SELECT * 
FROM "Y" 
WHERE "a" = @p0)