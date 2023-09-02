-------- ORIGINAL -----------

SELECT * 
FROM "X" 
WHERE NOT (TO_CHAR("a", 'HH24:MI:SS') = TO_CHAR('2000-01-02 03:04:05', 'HH24:MI:SS'))

----------- RAW -------------

SELECT * 
FROM "X" 
WHERE NOT (TO_CHAR("a", 'HH24:MI:SS') = TO_CHAR(?, 'HH24:MI:SS'))

--------PARAMETRIZED --------

SELECT * 
FROM "X" 
WHERE NOT (TO_CHAR("a", 'HH24:MI:SS') = TO_CHAR(:p0, 'HH24:MI:SS'))