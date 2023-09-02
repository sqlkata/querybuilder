-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE NOT (CAST("A" as TIME) = '2000-01-02 03:04:05')

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE NOT (CAST("A" as TIME) = ?)

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE NOT (CAST("A" as TIME) = @p0)