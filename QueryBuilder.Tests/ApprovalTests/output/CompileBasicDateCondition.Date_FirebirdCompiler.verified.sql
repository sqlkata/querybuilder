-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE NOT (CAST("A" as DATE) = '2000-01-02 03:04:05')

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE NOT (CAST("A" as DATE) = ?)

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE NOT (CAST("A" as DATE) = @p0)