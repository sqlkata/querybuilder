-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE NOT (TIME("a") = '2000-01-02 03:04:05')

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE NOT (TIME("a") = ?)

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE NOT (TIME("a") = @p0)