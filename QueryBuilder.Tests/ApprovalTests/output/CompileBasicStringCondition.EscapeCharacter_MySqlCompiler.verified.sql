-------- ORIGINAL -----------

SELECT * 
FROM `X` 
WHERE LOWER(`a`) like 'k*%' ESCAPE '*'

----------- RAW -------------

SELECT * 
FROM `X` 
WHERE LOWER(`a`) like ? ESCAPE '*'

--------PARAMETRIZED --------

SELECT * 
FROM `X` 
WHERE LOWER(`a`) like @p0 ESCAPE '*'