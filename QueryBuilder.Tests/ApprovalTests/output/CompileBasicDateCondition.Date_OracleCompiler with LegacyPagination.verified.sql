-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE TO_CHAR("a", 'YY-MM-DD') = TO_CHAR('2000-01-02 03:04:05', 'YY-MM-DD')

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE TO_CHAR("a", 'YY-MM-DD') = TO_CHAR(?, 'YY-MM-DD')

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE TO_CHAR("a", 'YY-MM-DD') = TO_CHAR(:p0, 'YY-MM-DD')