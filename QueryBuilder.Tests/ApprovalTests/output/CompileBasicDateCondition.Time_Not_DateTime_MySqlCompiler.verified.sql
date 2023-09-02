-------- ORIGINAL -----------

SELECT * 
FROM `X` 
WHERE TIME(`a`) = 'blah'

----------- RAW -------------

SELECT * 
FROM `X` 
WHERE TIME(`a`) = ?

--------PARAMETRIZED --------

SELECT * 
FROM `X` 
WHERE TIME(`a`) = @p0