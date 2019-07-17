IF	EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.spAddMethods') AND type in (N'P', N'PC'))
	DROP PROCEDURE dbo.spAddMethods
GO

IF	EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.LoggedMethods') AND type in (N'U', N'TT'))
	DROP TABLE dbo.LoggedMethods
GO

IF	EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'LogMethodParams' AND ss.name = N'dbo')
	DROP TYPE dbo.LogMethodParams
GO

CREATE TABLE dbo.LoggedMethods
(
	method VARCHAR(900) NOT NULL PRIMARY KEY CLUSTERED,
	method_part2 VARCHAR(900) NULL,
	executing_app VARCHAR(900) NOT NULL,
	pid INT NOT NULL,
	machine VARCHAR(200) NOT NULL,
	inserted_on DATETIME NOT NULL
)
GO

CREATE TYPE dbo.LogMethodParams AS TABLE
(
	method VARCHAR(900) NOT NULL,
	method_part2 VARCHAR(900) NULL,
	executing_app VARCHAR(900) NOT NULL,
	pid INT NOT NULL,
	machine VARCHAR(200) NOT NULL,
	inserted_on DATETIME NOT NULL
)
GO

CREATE PROCEDURE dbo.spAddMethods 
	@TableParam AS dbo.LogMethodParams READONLY
AS
BEGIN
	SET NOCOUNT ON;
	
	MERGE dbo.LoggedMethods WITH (HOLDLOCK) AS [target]
	USING @TableParam AS source
	ON	[target].method = source.method
		AND
		(
			([target].method_part2 IS NULL AND source.method_part2 IS NULL) 
			OR
			[target].method_part2 = source.method_part2
		)
	WHEN NOT MATCHED THEN
		INSERT 
		(
			method,
			method_part2,
			executing_app,
			pid,
			machine,
			inserted_on
		)
		VALUES
		(
			source.method,
			source.method_part2,
			source.executing_app,
			source.pid,
			source.machine,
			source.inserted_on
		);
	--WHEN MATCHED THEN ??????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????
	--	UPDATE
	--	SET
	--		executing_app = source.executing_app,
	--		machine = source.machine,
	--		pid = source.pid;
END
GO