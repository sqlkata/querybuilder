-------- ORIGINAL -----------

SELECT T("a") FILTER (WHERE "b" = 3) 
FROM "X"

----------- RAW -------------

SELECT T("a") FILTER (WHERE "b" = ?) 
FROM "X"

--------PARAMETRIZED --------

SELECT T("a") FILTER (WHERE "b" = @p0) 
FROM "X"