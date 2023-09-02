-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE CAST("A" as TIME) = 'blah'

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE CAST("A" as TIME) = ?

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE CAST("A" as TIME) = @p0