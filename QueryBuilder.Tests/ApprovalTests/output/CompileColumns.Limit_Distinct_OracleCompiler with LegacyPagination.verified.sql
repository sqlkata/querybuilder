-------- ORIGINAL -----------

SELECT * 
FROM (
SELECT DISTINCT * 
FROM "X") 
WHERE ROWNUM <= 3

----------- RAW -------------

SELECT * 
FROM (
SELECT DISTINCT * 
FROM "X") 
WHERE ROWNUM <= ?

--------PARAMETRIZED --------

SELECT * 
FROM (
SELECT DISTINCT * 
FROM "X") 
WHERE ROWNUM <= :p0