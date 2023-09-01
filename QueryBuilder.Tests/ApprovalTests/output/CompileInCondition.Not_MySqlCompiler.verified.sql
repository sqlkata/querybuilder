-------- ORIGINAL -----------

SELECT * 
FROM `X` 
WHERE `a` NOT IN ('0', '99')

----------- RAW -------------

SELECT * 
FROM `X` 
WHERE `a` NOT IN (?, ?)

--------PARAMETRIZED --------

SELECT * 
FROM `X` 
WHERE `a` NOT IN (@p0, @p1)