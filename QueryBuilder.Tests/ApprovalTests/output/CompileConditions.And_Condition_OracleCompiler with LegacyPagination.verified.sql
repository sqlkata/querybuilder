-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE "a" = 88 
AND "b" = 77

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE "a" = ? 
AND "b" = ?

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE "a" = :p0 
AND "b" = :p1