-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE "A" NOT IN (
SELECT * 
FROM "Y")

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE "A" NOT IN (
SELECT * 
FROM "Y")

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE "A" NOT IN (
SELECT * 
FROM "Y")