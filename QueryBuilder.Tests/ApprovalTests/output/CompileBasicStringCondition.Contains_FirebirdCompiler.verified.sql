-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE LOWER("A") like '%k%'

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE LOWER("A") like ?

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE LOWER("A") like @p0