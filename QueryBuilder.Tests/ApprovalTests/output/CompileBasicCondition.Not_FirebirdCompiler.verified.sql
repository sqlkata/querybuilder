-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE NOT ("A" = 'k')

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE NOT ("A" = ?)

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE NOT ("A" = @p0)