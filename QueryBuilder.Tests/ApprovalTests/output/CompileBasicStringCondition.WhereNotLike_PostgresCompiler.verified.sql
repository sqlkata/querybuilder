-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE NOT ("a" ilike 'K')

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE NOT ("a" ilike ?)

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE NOT ("a" ilike @p0)