-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE CAST("A" as DATE) = 'blah'

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE CAST("A" as DATE) = ?

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE CAST("A" as DATE) = @p0