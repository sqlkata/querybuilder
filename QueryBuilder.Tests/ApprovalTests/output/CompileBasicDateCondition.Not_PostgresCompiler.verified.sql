-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE NOT (DATE_PART('YEAR', "a") = '2000-01-02 03:04:05')

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE NOT (DATE_PART('YEAR', "a") = ?)

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE NOT (DATE_PART('YEAR', "a") = @p0)