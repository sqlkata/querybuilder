-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE NOT (EXTRACT(YEAR 
FROM "a") = '2000-01-02 03:04:05')

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE NOT (EXTRACT(YEAR 
FROM "a") = ?)

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE NOT (EXTRACT(YEAR 
FROM "a") = :p0)