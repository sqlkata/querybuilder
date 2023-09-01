-------- ORIGINAL -----------

SELECT * 
FROM `X` 
WHERE NOT (LOWER(`a`) like 'k')

----------- RAW -------------

SELECT * 
FROM `X` 
WHERE NOT (LOWER(`a`) like ?)

--------PARAMETRIZED --------

SELECT * 
FROM `X` 
WHERE NOT (LOWER(`a`) like @p0)