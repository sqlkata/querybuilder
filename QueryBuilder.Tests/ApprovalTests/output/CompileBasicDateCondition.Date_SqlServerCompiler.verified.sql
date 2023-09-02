-------- ORIGINAL -----------

SELECT * 
FROM [X] 
WHERE CAST([a] AS DATE) = '2000-01-02 03:04:05'

----------- RAW -------------

SELECT * 
FROM [X] 
WHERE CAST([a] AS DATE) = ?

--------PARAMETRIZED --------

SELECT * 
FROM [X] 
WHERE CAST([a] AS DATE) = @p0