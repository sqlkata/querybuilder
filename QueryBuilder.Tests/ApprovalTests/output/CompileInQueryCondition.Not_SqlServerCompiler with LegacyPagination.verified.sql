-------- ORIGINAL -----------

SELECT * 
FROM [X] 
WHERE [a] NOT IN (
SELECT * 
FROM [Y])

----------- RAW -------------

SELECT * 
FROM [X] 
WHERE [a] NOT IN (
SELECT * 
FROM [Y])

--------PARAMETRIZED --------

SELECT * 
FROM [X] 
WHERE [a] NOT IN (
SELECT * 
FROM [Y])