-------- ORIGINAL -----------

SELECT * 
FROM `X` 
WHERE `a` = 88 
OR `b` = 77

----------- RAW -------------

SELECT * 
FROM `X` 
WHERE `a` = ? 
OR `b` = ?

--------PARAMETRIZED --------

SELECT * 
FROM `X` 
WHERE `a` = @p0 
OR `b` = @p1