-------- ORIGINAL -----------

SELECT * 
FROM [X] 
WHERE [a] NOT BETWEEN '0' 
AND '99'

----------- RAW -------------

SELECT * 
FROM [X] 
WHERE [a] NOT BETWEEN ? 
AND ?

--------PARAMETRIZED --------

SELECT * 
FROM [X] 
WHERE [a] NOT BETWEEN @p0 
AND @p1