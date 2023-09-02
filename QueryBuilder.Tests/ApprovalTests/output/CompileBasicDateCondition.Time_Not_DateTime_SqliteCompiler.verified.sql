-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE strftime('%H:%M:%S', "a") = cast('blah' as text)

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE strftime('%H:%M:%S', "a") = cast(? as text)

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE strftime('%H:%M:%S', "a") = cast(@p0 as text)