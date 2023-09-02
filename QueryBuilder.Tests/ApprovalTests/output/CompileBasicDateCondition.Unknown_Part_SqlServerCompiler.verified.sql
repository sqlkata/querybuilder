-------- ORIGINAL -----------

SELECT * 
FROM [X] 
WHERE DATEPART(BLAH, [a]) = 1

----------- RAW -------------

SELECT * 
FROM [X] 
WHERE DATEPART(BLAH, [a]) = ?

--------PARAMETRIZED --------

SELECT * 
FROM [X] 
WHERE DATEPART(BLAH, [a]) = @p0