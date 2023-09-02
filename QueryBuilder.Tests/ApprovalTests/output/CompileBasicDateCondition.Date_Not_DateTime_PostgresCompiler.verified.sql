-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE NOT ("a"::date = 'blah')

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE NOT ("a"::date = ?)

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE NOT ("a"::date = @p0)