-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE NOT (strftime('%Y', "a") = cast('2000-01-02 03:04:05' as text))

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE NOT (strftime('%Y', "a") = cast(? as text))

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE NOT (strftime('%Y', "a") = cast(@p0 as text))