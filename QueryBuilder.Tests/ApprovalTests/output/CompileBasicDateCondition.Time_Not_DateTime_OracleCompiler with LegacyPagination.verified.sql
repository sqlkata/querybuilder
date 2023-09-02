-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE TO_CHAR("a", 'HH24:MI:SS') = TO_CHAR(TO_DATE('blah', 'HH24:MI:SS'), 'HH24:MI:SS')

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE TO_CHAR("a", 'HH24:MI:SS') = TO_CHAR(TO_DATE(?, 'HH24:MI:SS'), 'HH24:MI:SS')

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE TO_CHAR("a", 'HH24:MI:SS') = TO_CHAR(TO_DATE(:p0, 'HH24:MI:SS'), 'HH24:MI:SS')