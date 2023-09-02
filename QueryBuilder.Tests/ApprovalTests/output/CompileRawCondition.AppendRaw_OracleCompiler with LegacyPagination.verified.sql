-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE blah 1 2 3

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE blah ? ? ?

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE blah :p0 :p1 :p2