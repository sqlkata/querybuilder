-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE "A" = 88 
OR "B" = 77

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE "A" = ? 
OR "B" = ?

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE "A" = @p0 
OR "B" = @p1