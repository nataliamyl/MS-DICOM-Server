SET XACT_ABORT ON
BEGIN TRANSACTION

DROP PROCEDURE IF EXISTS dbo.GetInstanceBatchesByTimeStamp
GO

COMMIT TRANSACTION

DROP INDEX IF EXISTS IX_Instance_Watermark_Status_CreatedDate ON dbo.Instance
GO


