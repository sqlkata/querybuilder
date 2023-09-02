-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE EXTRACT("BLAH" 
FROM "A") = 1

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE EXTRACT("BLAH" 
FROM "A") = ?

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE EXTRACT("BLAH" 
FROM "A") = @p0