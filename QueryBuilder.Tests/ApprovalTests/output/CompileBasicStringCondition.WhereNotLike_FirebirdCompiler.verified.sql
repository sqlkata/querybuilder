-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE NOT (LOWER("A") like 'k')

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE NOT (LOWER("A") like ?)

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE NOT (LOWER("A") like @p0)