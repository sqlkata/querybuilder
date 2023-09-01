-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE NOT ("a" = 'k')

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE NOT ("a" = ?)

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE NOT ("a" = :p0)