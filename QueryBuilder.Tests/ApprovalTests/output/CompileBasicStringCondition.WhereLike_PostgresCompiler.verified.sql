-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE "a" ilike 'k'

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE "a" ilike ?

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE "a" ilike @p0