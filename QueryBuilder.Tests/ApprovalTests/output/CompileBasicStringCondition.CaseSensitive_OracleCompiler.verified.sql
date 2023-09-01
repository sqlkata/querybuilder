-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE "a" like 'K%'

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE "a" like ?

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE "a" like :p0