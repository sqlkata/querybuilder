-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE "A" = 88 
AND "B" = 77

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE "A" = ? 
AND "B" = ?

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE "A" = @p0 
AND "B" = @p1