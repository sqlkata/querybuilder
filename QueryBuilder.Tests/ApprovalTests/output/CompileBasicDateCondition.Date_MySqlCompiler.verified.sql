-------- ORIGINAL -----------

SELECT * 
FROM `X` 
WHERE DATE(`a`) = '2000-01-02 03:04:05'

----------- RAW -------------

SELECT * 
FROM `X` 
WHERE DATE(`a`) = ?

--------PARAMETRIZED --------

SELECT * 
FROM `X` 
WHERE DATE(`a`) = @p0