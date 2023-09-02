-------- ORIGINAL -----------

SELECT * 
FROM (
SELECT "results_wrapper".*, ROWNUM "row_num" 
FROM (
SELECT *) "results_wrapper" 
WHERE ROWNUM <= 8) 
WHERE "row_num" > 7

----------- RAW -------------

SELECT * 
FROM (
SELECT "results_wrapper".*, ROWNUM "row_num" 
FROM (
SELECT *) "results_wrapper" 
WHERE ROWNUM <= ?) 
WHERE "row_num" > ?

--------PARAMETRIZED --------

SELECT * 
FROM (
SELECT "results_wrapper".*, ROWNUM "row_num" 
FROM (
SELECT *) "results_wrapper" 
WHERE ROWNUM <= :p0) 
WHERE "row_num" > :p1