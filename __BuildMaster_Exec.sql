CREATE PROCEDURE [__BuildMaster_Exec]
(
	@SchemaVersion_Id BIGINT,
	@Script_Name NVARCHAR(200),
	@Script_Sql NTEXT
) AS BEGIN

	SET NOCOUNT ON

	-- Validate Input
	IF @SchemaVersion_Id IS NULL OR 
       NULLIF(@Script_Name,'') IS NULL OR 
       @Script_Sql IS NULL OR 
       @Script_Sql LIKE '' BEGIN
		PRINT 'Script not run: no parameters may be null or empty.'
		RETURN
	END

	-- Create Schema Changes Table
	IF OBJECT_ID('__BuildMaster_SchemaChanges') IS NULL BEGIN
		CREATE TABLE __BuildMaster_SchemaChanges (
		  [SchemaVersion_Id] BIGINT NOT NULL,
		  [Script_Name] NVARCHAR(200) NOT NULL,
		  [Executed_Date] DATETIME NOT NULL,
		  CONSTRAINT [__BuildMaster_SchemaChangesPK]
			PRIMARY KEY ([SchemaVersion_Id], [Script_Name])
		)
		INSERT INTO [__BuildMaster_SchemaChanges]
			([SchemaVersion_Id], [Script_Name], [Executed_Date])
		VALUES
			(@SchemaVersion_Id, 'CREATE TABLE __BuildMaster_SchemaChanges', GETDATE())
		PRINT 'Schema table created.'
	END

	-- Get Latest Schema Version
	DECLARE @LatestVersion BIGINT
	    SET @LatestVersion = COALESCE(
			(SELECT MAX([SchemaVersion_Id]) FROM [__BuildMaster_SchemaChanges]),
			@SchemaVersion_Id)
	PRINT 'Latest schema version is ' + CAST(@LatestVersion AS VARCHAR(20)) + '.'

	-- Don't run if older version
	IF @SchemaVersion_Id < @LatestVersion BEGIN
		PRINT 'Script "' + @Script_Name + '" skipped: older version.'
		RETURN
	END

	IF EXISTS(SELECT * FROM [__BuildMaster_SchemaChanges]
	           WHERE [SchemaVersion_Id] = @SchemaVersion_Id
	             AND [Script_Name] = @Script_Name) BEGIN
		PRINT 'Script "' + @Script_Name + '" skipped: already ran.'
		RETURN
	END

	-- Exec Script and run
	EXEC sp_executesql @Script_Sql
	IF @@ERROR=0 BEGIN
		INSERT INTO [__BuildMaster_SchemaChanges]
			([SchemaVersion_Id], [Script_Name], [Executed_Date])
		VALUES
			(@SchemaVersion_Id, @Script_Name, GETDATE())
		PRINT 'Script "' + @Script_Name + '" executed successfully.'
	END ELSE BEGIN
		PRINT 'Script "' + @Script_Name + '" failed.'
	END
END