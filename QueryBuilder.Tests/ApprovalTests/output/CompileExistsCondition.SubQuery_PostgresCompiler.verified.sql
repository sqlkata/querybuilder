-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE NOT EXISTS (
SELECT 1 
FROM "Y")

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE NOT EXISTS (
SELECT 1 
FROM "Y")

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE NOT EXISTS (
SELECT 1 
FROM "Y")