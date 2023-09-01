-------- ORIGINAL -----------

SELECT * 
FROM [X] 
WHERE [a] = (
SELECT * 
FROM [Y])

----------- RAW -------------

SELECT * 
FROM [X] 
WHERE [a] = (
SELECT * 
FROM [Y])

--------PARAMETRIZED --------

SELECT * 
FROM [X] 
WHERE [a] = (
SELECT * 
FROM [Y])