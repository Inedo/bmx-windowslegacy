CREATE TABLE __BuildMaster_DbSchemaChanges (
  [Numeric_Release_Number] BIGINT NOT NULL,
  [Script_Id] INT NOT NULL,
  [Script_Sequence] INT NOT NULL,

  [Batch_Name] NVARCHAR(50) NOT NULL,
  [Executed_Date] DATETIME NOT NULL,
  [Success_Indicator] CHAR(1) NOT NULL,

  CONSTRAINT [__BuildMaster_DbSchemaChangesPK]
	PRIMARY KEY ([Numeric_Release_Number], [Script_Id], [Script_Sequence])
)
GO

INSERT INTO [__BuildMaster_DbSchemaChanges]
	([Numeric_Release_Number], [Script_Id], [Script_Sequence], [Batch_Name], [Executed_Date], [Success_Indicator])
VALUES
	(0, 0, 1, 'CREATE TABLE __BuildMaster_DbSchemaChanges', GETDATE(), 'Y')
PRINT 'Schema table created.'
GO