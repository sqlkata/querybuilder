-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE NOT (EXTRACT("YEAR" 
FROM "A") = '2000-01-02 03:04:05')

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE NOT (EXTRACT("YEAR" 
FROM "A") = ?)

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE NOT (EXTRACT("YEAR" 
FROM "A") = @p0)