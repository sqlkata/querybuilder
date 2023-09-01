-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE LOWER("A") like 'k*%' ESCAPE '*'

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE LOWER("A") like ? ESCAPE '*'

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE LOWER("A") like @p0 ESCAPE '*'