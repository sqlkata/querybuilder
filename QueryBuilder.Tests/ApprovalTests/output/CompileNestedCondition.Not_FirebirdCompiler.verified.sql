-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE NOT ("A" = 632)

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE NOT ("A" = ?)

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE NOT ("A" = @p0)