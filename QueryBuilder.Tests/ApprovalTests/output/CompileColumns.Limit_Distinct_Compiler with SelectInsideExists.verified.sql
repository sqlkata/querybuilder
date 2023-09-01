-------- ORIGINAL -----------

SELECT DISTINCT * 
FROM "X" LIMIT 3

----------- RAW -------------

SELECT DISTINCT * 
FROM "X" LIMIT ?

--------PARAMETRIZED --------

SELECT DISTINCT * 
FROM "X" LIMIT @p0