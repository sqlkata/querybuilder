-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE NOT ("a"::time = '2000-01-02 03:04:05')

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE NOT ("a"::time = ?)

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE NOT ("a"::time = @p0)