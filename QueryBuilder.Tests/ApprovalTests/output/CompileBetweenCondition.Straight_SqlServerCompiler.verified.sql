-------- ORIGINAL -----------

SELECT * 
FROM [X] 
WHERE [a] BETWEEN 'aaa' 
AND 'zzz'

----------- RAW -------------

SELECT * 
FROM [X] 
WHERE [a] BETWEEN ? 
AND ?

--------PARAMETRIZED --------

SELECT * 
FROM [X] 
WHERE [a] BETWEEN @p0 
AND @p1