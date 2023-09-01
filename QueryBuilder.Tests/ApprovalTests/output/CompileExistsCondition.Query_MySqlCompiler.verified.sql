-------- ORIGINAL -----------

SELECT * 
FROM `X` 
WHERE EXISTS (
SELECT 1 
FROM `Y` 
WHERE `a` = 4)

----------- RAW -------------

SELECT * 
FROM `X` 
WHERE EXISTS (
SELECT 1 
FROM `Y` 
WHERE `a` = ?)

--------PARAMETRIZED --------

SELECT * 
FROM `X` 
WHERE EXISTS (
SELECT 1 
FROM `Y` 
WHERE `a` = @p0)