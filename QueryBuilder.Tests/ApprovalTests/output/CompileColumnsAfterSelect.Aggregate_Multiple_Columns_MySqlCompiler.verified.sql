-------- ORIGINAL -----------

SELECT COUNT(*) AS `count` 
FROM (
SELECT 1 
FROM `X` WHERE `a` IS NOT NULL AND `b` IS NOT NULL) AS `countQuery`

----------- RAW -------------

SELECT COUNT(*) AS `count` 
FROM (
SELECT 1 
FROM `X` WHERE `a` IS NOT NULL AND `b` IS NOT NULL) AS `countQuery`

--------PARAMETRIZED --------

SELECT COUNT(*) AS `count` 
FROM (
SELECT 1 
FROM `X` WHERE `a` IS NOT NULL AND `b` IS NOT NULL) AS `countQuery`