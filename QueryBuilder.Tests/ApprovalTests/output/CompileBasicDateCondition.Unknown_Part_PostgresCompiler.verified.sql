-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE DATE_PART('BLAH', "a") = 1

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE DATE_PART('BLAH', "a") = ?

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE DATE_PART('BLAH', "a") = @p0