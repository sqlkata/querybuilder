-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE "A" NOT IN ('0', '99')

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE "A" NOT IN (?, ?)

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE "A" NOT IN (@p0, @p1)