-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE BLAH("a") = 1

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE BLAH("a") = ?

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE BLAH("a") = @p0