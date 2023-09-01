-------- ORIGINAL -----------

SELECT * 
FROM "X" LIMIT -1 OFFSET 4

----------- RAW -------------

SELECT * 
FROM "X" LIMIT -1 OFFSET ?

--------PARAMETRIZED --------

SELECT * 
FROM "X" LIMIT -1 OFFSET @p0