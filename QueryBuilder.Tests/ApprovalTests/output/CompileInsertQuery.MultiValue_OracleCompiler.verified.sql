-------- ORIGINAL -----------
INSERT ALL INTO "X" ("a") 
VALUES (1) INTO "X" ("a") 
VALUES (2) 
SELECT 1 
FROM DUAL

----------- RAW -------------
INSERT ALL INTO "X" ("a") 
VALUES (?) INTO "X" ("a") 
VALUES (?) 
SELECT 1 
FROM DUAL

--------PARAMETRIZED --------
INSERT ALL INTO "X" ("a") 
VALUES (:p0) INTO "X" ("a") 
VALUES (:p1) 
SELECT 1 
FROM DUAL