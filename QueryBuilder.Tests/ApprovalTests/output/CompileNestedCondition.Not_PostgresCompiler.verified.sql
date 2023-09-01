-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE NOT ("a" = 632)

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE NOT ("a" = ?)

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE NOT ("a" = @p0)