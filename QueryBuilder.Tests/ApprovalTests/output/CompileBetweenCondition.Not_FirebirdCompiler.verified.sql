-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE "A" NOT BETWEEN '0' 
AND '99'

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE "A" NOT BETWEEN ? 
AND ?

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE "A" NOT BETWEEN @p0 
AND @p1