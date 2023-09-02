-------- ORIGINAL -----------

SELECT * 
FROM (
SELECT * 
FROM "X") 
WHERE ROWNUM <= 3

----------- RAW -------------

SELECT * 
FROM (
SELECT * 
FROM "X") 
WHERE ROWNUM <= ?

--------PARAMETRIZED --------

SELECT * 
FROM (
SELECT * 
FROM "X") 
WHERE ROWNUM <= :p0