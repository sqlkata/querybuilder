-------- ORIGINAL -----------

SELECT * 
FROM [X] 
WHERE [a] = cast(1 as bit) 
OR [b] = cast(0 as bit)

----------- RAW -------------

SELECT * 
FROM [X] 
WHERE [a] = cast(1 as bit) 
OR [b] = cast(0 as bit)

--------PARAMETRIZED --------

SELECT * 
FROM [X] 
WHERE [a] = cast(1 as bit) 
OR [b] = cast(0 as bit)