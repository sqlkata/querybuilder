-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE NOT (DATE("a") = 'blah')

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE NOT (DATE("a") = ?)

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE NOT (DATE("a") = @p0)