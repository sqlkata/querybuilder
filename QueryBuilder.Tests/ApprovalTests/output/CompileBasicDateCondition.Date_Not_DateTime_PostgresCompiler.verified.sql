-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE "a"::date = 'blah'

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE "a"::date = ?

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE "a"::date = @p0