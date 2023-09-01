-------- ORIGINAL -----------

SELECT * 
FROM (
SELECT *, ROW_NUMBER() OVER (
ORDER BY (
SELECT 0)) AS [row_num] 
FROM [X]) AS [results_wrapper] 
WHERE [row_num] >= 5

----------- RAW -------------

SELECT * 
FROM (
SELECT *, ROW_NUMBER() OVER (
ORDER BY (
SELECT 0)) AS [row_num] 
FROM [X]) AS [results_wrapper] 
WHERE [row_num] >= ?

--------PARAMETRIZED --------

SELECT * 
FROM (
SELECT *, ROW_NUMBER() OVER (
ORDER BY (
SELECT 0)) AS [row_num] 
FROM [X]) AS [results_wrapper] 
WHERE [row_num] >= @p0