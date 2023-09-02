-------- ORIGINAL -----------

SELECT * 
FROM [X] 
WHERE CAST([a] AS TIME) = 'blah'

----------- RAW -------------

SELECT * 
FROM [X] 
WHERE CAST([a] AS TIME) = ?

--------PARAMETRIZED --------

SELECT * 
FROM [X] 
WHERE CAST([a] AS TIME) = @p0