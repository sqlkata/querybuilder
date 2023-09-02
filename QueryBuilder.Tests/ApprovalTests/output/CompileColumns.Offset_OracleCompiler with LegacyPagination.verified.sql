-------- ORIGINAL -----------

SELECT * 
FROM (
SELECT "results_wrapper".*, ROWNUM "row_num" 
FROM (
SELECT * 
FROM "X") "results_wrapper") 
WHERE "row_num" > 4

----------- RAW -------------

SELECT * 
FROM (
SELECT "results_wrapper".*, ROWNUM "row_num" 
FROM (
SELECT * 
FROM "X") "results_wrapper") 
WHERE "row_num" > ?

--------PARAMETRIZED --------

SELECT * 
FROM (
SELECT "results_wrapper".*, ROWNUM "row_num" 
FROM (
SELECT * 
FROM "X") "results_wrapper") 
WHERE "row_num" > :p0