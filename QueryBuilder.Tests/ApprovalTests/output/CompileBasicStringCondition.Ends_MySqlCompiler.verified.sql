-------- ORIGINAL -----------

SELECT * 
FROM `X` 
WHERE LOWER(`a`) like '%k'

----------- RAW -------------

SELECT * 
FROM `X` 
WHERE LOWER(`a`) like ?

--------PARAMETRIZED --------

SELECT * 
FROM `X` 
WHERE LOWER(`a`) like @p0