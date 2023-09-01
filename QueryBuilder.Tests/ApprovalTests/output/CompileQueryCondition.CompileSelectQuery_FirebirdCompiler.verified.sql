-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE "A" = (
SELECT * 
FROM "Y")

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE "A" = (
SELECT * 
FROM "Y")

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE "A" = (
SELECT * 
FROM "Y")