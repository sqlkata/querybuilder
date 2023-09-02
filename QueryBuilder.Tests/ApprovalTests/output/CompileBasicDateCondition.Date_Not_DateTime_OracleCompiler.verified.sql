-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE NOT (TO_CHAR("a", 'YY-MM-DD') = TO_CHAR(TO_DATE('blah', 'YY-MM-DD'), 'YY-MM-DD'))

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE NOT (TO_CHAR("a", 'YY-MM-DD') = TO_CHAR(TO_DATE(?, 'YY-MM-DD'), 'YY-MM-DD'))

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE NOT (TO_CHAR("a", 'YY-MM-DD') = TO_CHAR(TO_DATE(:p0, 'YY-MM-DD'), 'YY-MM-DD'))