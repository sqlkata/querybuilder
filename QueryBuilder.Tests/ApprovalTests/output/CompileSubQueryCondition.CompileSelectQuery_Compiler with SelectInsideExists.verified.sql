-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE (
SELECT * 
FROM "Y") = 52

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE (
SELECT * 
FROM "Y") = ?

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE (
SELECT * 
FROM "Y") = @p0