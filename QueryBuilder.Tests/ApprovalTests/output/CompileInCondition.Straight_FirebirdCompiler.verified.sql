-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE "A" IN ('aaa', 'zzz')

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE "A" IN (?, ?)

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE "A" IN (@p0, @p1)