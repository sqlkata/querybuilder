-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE NOT ("a"::date = '2000-01-02 03:04:05')

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE NOT ("a"::date = ?)

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE NOT ("a"::date = @p0)