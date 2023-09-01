-------- ORIGINAL -----------

SELECT T(CASE WHEN [b] = 3 THEN [a] END) FROM [X]

----------- RAW -------------

SELECT T(CASE WHEN [b] = ? THEN [a] END) FROM [X]

--------PARAMETRIZED --------

SELECT T(CASE WHEN [b] = @p0 THEN [a] END) FROM [X]