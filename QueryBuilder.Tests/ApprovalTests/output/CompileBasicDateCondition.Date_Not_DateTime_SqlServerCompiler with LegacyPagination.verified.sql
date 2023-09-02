-------- ORIGINAL -----------

SELECT * 
FROM [X] 
WHERE CAST([a] AS DATE) = 'blah'

----------- RAW -------------

SELECT * 
FROM [X] 
WHERE CAST([a] AS DATE) = ?

--------PARAMETRIZED --------

SELECT * 
FROM [X] 
WHERE CAST([a] AS DATE) = @p0