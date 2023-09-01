-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE "A" BETWEEN 'aaa' 
AND 'zzz'

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE "A" BETWEEN ? 
AND ?

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE "A" BETWEEN @p0 
AND @p1