-------- ORIGINAL -----------

SELECT * 
FROM [X] 
WHERE NOT (CAST([a] AS DATE) = 'blah')

----------- RAW -------------

SELECT * 
FROM [X] 
WHERE NOT (CAST([a] AS DATE) = ?)

--------PARAMETRIZED --------

SELECT * 
FROM [X] 
WHERE NOT (CAST([a] AS DATE) = @p0)