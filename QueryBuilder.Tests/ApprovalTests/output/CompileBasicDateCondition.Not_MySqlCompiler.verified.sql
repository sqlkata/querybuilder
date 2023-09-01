-------- ORIGINAL -----------

SELECT * 
FROM `X` 
WHERE NOT (YEAR(`a`) = '2000-01-02 03:04:05')

----------- RAW -------------

SELECT * 
FROM `X` 
WHERE NOT (YEAR(`a`) = ?)

--------PARAMETRIZED --------

SELECT * 
FROM `X` 
WHERE NOT (YEAR(`a`) = @p0)