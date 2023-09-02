-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE "a" ilike 'K*%' ESCAPE '*'

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE "a" ilike ? ESCAPE '*'

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE "a" ilike @p0 ESCAPE '*'