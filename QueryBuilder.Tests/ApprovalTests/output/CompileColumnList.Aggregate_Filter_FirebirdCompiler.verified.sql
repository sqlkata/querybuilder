-------- ORIGINAL -----------

SELECT T(CASE WHEN "B" = 3 THEN "A" END) 
FROM "X"

----------- RAW -------------

SELECT T(CASE WHEN "B" = ? THEN "A" END) 
FROM "X"

--------PARAMETRIZED --------

SELECT T(CASE WHEN "B" = @p0 THEN "A" END) 
FROM "X"