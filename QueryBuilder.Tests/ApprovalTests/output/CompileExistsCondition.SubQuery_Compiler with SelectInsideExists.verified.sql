-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE NOT EXISTS (
SELECT * 
FROM "Y")

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE NOT EXISTS (
SELECT * 
FROM "Y")

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE NOT EXISTS (
SELECT * 
FROM "Y")