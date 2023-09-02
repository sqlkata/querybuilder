-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE strftime('%Y-%m-%d', "a") = cast('blah' as text)

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE strftime('%Y-%m-%d', "a") = cast(? as text)

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE strftime('%Y-%m-%d', "a") = cast(@p0 as text)