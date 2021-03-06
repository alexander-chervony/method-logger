SELECT distinct machine
  FROM [MethodLogging].[dbo].[LoggedMethods] order by machine
  
SELECT distinct [assembly]
  FROM [MethodLogging].[dbo].[LoggedMethods] order by [assembly]

SELECT count(*)
  FROM [MethodLogging].[dbo].[LoggedMethods]

SELECT top 1000 * FROM [MethodLogging].[dbo].[LoggedMethods] where method not like '<>%' and method like '%<>%'


SELECT count(*) FROM [MethodLogging].[dbo].[LoggedMethods]
SELECT count(*) FROM [MethodLogging].[dbo].[LoggedMethods] where method not like '%>%' and method not like '%<%'


SELECT top 1000 * FROM [MethodLogging].[dbo].[LoggedMethods] where method like '%>%'