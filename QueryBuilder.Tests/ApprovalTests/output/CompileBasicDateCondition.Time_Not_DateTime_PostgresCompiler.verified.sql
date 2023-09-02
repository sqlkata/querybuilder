-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE "a"::time = 'blah'

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE "a"::time = ?

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE "a"::time = @p0