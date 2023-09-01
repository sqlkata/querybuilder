-------- ORIGINAL -----------

SELECT COUNT(*) "count" FROM (SELECT 1 FROM "X" WHERE "a" IS NOT NULL AND "b" IS NOT NULL) "countQuery"

----------- RAW -------------

SELECT COUNT(*) "count" FROM (SELECT 1 FROM "X" WHERE "a" IS NOT NULL AND "b" IS NOT NULL) "countQuery"

--------PARAMETRIZED --------

SELECT COUNT(*) "count" FROM (SELECT 1 FROM "X" WHERE "a" IS NOT NULL AND "b" IS NOT NULL) "countQuery"