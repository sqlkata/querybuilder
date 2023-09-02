-------- ORIGINAL -----------

SELECT * 
FROM (
SELECT *, ROW_NUMBER() OVER (
ORDER BY (
SELECT 0)) AS [row_num]) AS [results_wrapper] 
WHERE [row_num] BETWEEN 8 
AND 8

----------- RAW -------------

SELECT * 
FROM (
SELECT *, ROW_NUMBER() OVER (
ORDER BY (
SELECT 0)) AS [row_num]) AS [results_wrapper] 
WHERE [row_num] BETWEEN ? 
AND ?

--------PARAMETRIZED --------

SELECT * 
FROM (
SELECT *, ROW_NUMBER() OVER (
ORDER BY (
SELECT 0)) AS [row_num]) AS [results_wrapper] 
WHERE [row_num] BETWEEN @p0 
AND @p1