-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE "a" = 1

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE "a" = ?

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE "a" = @p0