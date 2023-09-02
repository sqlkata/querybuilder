-------- ORIGINAL -----------

SELECT * 
FROM [X] 
WHERE NOT (CAST([a] AS TIME) = '2000-01-02 03:04:05')

----------- RAW -------------

SELECT * 
FROM [X] 
WHERE NOT (CAST([a] AS TIME) = ?)

--------PARAMETRIZED --------

SELECT * 
FROM [X] 
WHERE NOT (CAST([a] AS TIME) = @p0)