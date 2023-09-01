-------- ORIGINAL -----------

SELECT * 
FROM `X` 
WHERE `a` IN ('aaa', 'zzz')

----------- RAW -------------

SELECT * 
FROM `X` 
WHERE `a` IN (?, ?)

--------PARAMETRIZED --------

SELECT * 
FROM `X` 
WHERE `a` IN (@p0, @p1)