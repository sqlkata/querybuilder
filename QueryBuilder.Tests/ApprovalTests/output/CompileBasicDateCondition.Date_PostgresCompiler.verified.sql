-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE "a"::date = '2000-01-02 03:04:05'

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE "a"::date = ?

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE "a"::date = @p0