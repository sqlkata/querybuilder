-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE "A" like 'K%'

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE "A" like ?

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE "A" like @p0