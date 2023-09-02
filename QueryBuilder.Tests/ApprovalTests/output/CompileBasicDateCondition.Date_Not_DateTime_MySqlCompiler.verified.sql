-------- ORIGINAL -----------

SELECT * 
FROM `X` 
WHERE DATE(`a`) = 'blah'

----------- RAW -------------

SELECT * 
FROM `X` 
WHERE DATE(`a`) = ?

--------PARAMETRIZED --------

SELECT * 
FROM `X` 
WHERE DATE(`a`) = @p0