﻿SET XACT_ABORT ON

BEGIN TRANSACTION
/*************************************************************
    ChangeFeed Table
    Add FilePath column
**************************************************************/
IF NOT EXISTS 
(
    SELECT *
    FROM    sys.columns
    WHERE   (NAME = 'FilePath')
        AND Object_id = OBJECT_ID('dbo.ChangeFeed')
)
BEGIN
    ALTER TABLE dbo.ChangeFeed 
    ADD FilePath NVARCHAR(4000) NULL
END
GO

/*************************************************************
    sproc updates
**************************************************************/

CREATE OR ALTER PROCEDURE dbo.GetChangeFeedLatestByTimeV39
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT   TOP (1) c.Sequence,
                     c.Timestamp,
                     c.Action,
                     p.PartitionName,
                     c.StudyInstanceUid,
                     c.SeriesInstanceUid,
                     c.SopInstanceUid,
                     c.OriginalWatermark,
                     c.CurrentWatermark,
                     c.FilePath
    FROM     dbo.ChangeFeed AS c WITH (HOLDLOCK)
             INNER JOIN
             dbo.Partition AS p
             ON p.PartitionKey = c.PartitionKey

    ORDER BY c.Timestamp DESC, c.Sequence DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.GetChangeFeedLatestV39
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT   TOP (1) c.Sequence,
                     c.Timestamp,
                     c.Action,
                     p.PartitionName,
                     c.StudyInstanceUid,
                     c.SeriesInstanceUid,
                     c.SopInstanceUid,
                     c.OriginalWatermark,
                     c.CurrentWatermark,
                     c.FilePath
    FROM     dbo.ChangeFeed AS c WITH (HOLDLOCK)
             INNER JOIN
             dbo.Partition AS p
             ON p.PartitionKey = c.PartitionKey
    ORDER BY c.Sequence DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.GetChangeFeedV39
@limit INT, @offset BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT   c.Sequence,
             c.Timestamp,
             c.Action,
             p.PartitionName,
             c.StudyInstanceUid,
             c.SeriesInstanceUid,
             c.SopInstanceUid,
             c.OriginalWatermark,
             c.CurrentWatermark,
             c.FilePath
    FROM     dbo.ChangeFeed AS c WITH (HOLDLOCK)
             INNER JOIN
             dbo.Partition AS p
             ON p.PartitionKey = c.PartitionKey
    WHERE    c.Sequence BETWEEN @offset + 1 AND @offset + @limit
    ORDER BY c.Sequence;
END
GO

CREATE OR ALTER PROCEDURE dbo.GetChangeFeedByTimeV39
@startTime DATETIMEOFFSET (7), @endTime DATETIMEOFFSET (7), @limit INT, @offset BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT   c.Sequence,
             c.Timestamp,
             c.Action,
             p.PartitionName,
             c.StudyInstanceUid,
             c.SeriesInstanceUid,
             c.SopInstanceUid,
             c.OriginalWatermark,
             c.CurrentWatermark,
             c.FilePath
    FROM     dbo.ChangeFeed AS c WITH (HOLDLOCK)
             INNER JOIN
             dbo.Partition AS p
             ON p.PartitionKey = c.PartitionKey
    WHERE    c.Timestamp >= @startTime
             AND c.Timestamp < @endTime
    ORDER BY c.Timestamp, c.Sequence
    OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY;
END
GO

-- On deleting instance, also delete the corresponding file properties
CREATE OR ALTER PROCEDURE dbo.DeleteInstanceV6
@cleanupAfter DATETIMEOFFSET (0), @createdStatus TINYINT, @partitionKey INT, @studyInstanceUid VARCHAR (64), @seriesInstanceUid VARCHAR (64)=NULL, @sopInstanceUid VARCHAR (64)=NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRANSACTION;
    DECLARE @deletedInstances AS TABLE (
        PartitionKey      INT         ,
        StudyInstanceUid  VARCHAR (64),
        SeriesInstanceUid VARCHAR (64),
        SopInstanceUid    VARCHAR (64),
        Status            TINYINT     ,
        Watermark         BIGINT      ,
        OriginalWatermark BIGINT      );
    DECLARE @studyKey AS BIGINT;
    DECLARE @seriesKey AS BIGINT;
    DECLARE @instanceKey AS BIGINT;
    DECLARE @deletedDate AS DATETIME2 = SYSUTCDATETIME();
    DECLARE @imageResourceType AS TINYINT = 0;
    SELECT @studyKey = StudyKey,
           @seriesKey = CASE @seriesInstanceUid WHEN NULL THEN NULL ELSE SeriesKey END,
           @instanceKey = CASE @sopInstanceUid WHEN NULL THEN NULL ELSE InstanceKey END
    FROM   dbo.Instance
    WHERE  PartitionKey = @partitionKey
           AND StudyInstanceUid = @studyInstanceUid
           AND SeriesInstanceUid = ISNULL(@seriesInstanceUid, SeriesInstanceUid)
           AND SopInstanceUid = ISNULL(@sopInstanceUid, SopInstanceUid);
    DELETE dbo.Instance
    OUTPUT deleted.PartitionKey, deleted.StudyInstanceUid, deleted.SeriesInstanceUid, deleted.SopInstanceUid, deleted.Status, deleted.Watermark, deleted.OriginalWatermark INTO @deletedInstances
    WHERE  PartitionKey = @partitionKey
           AND StudyInstanceUid = @studyInstanceUid
           AND SeriesInstanceUid = ISNULL(@seriesInstanceUid, SeriesInstanceUid)
           AND SopInstanceUid = ISNULL(@sopInstanceUid, SopInstanceUid);
    IF @@ROWCOUNT = 0
        THROW 50404, 'Instance not found', 1;
    DECLARE @deletedTags AS TABLE (
        TagKey BIGINT);
    DELETE XQTE
    OUTPUT deleted.TagKey INTO @deletedTags
    FROM   dbo.ExtendedQueryTagError AS XQTE
           INNER JOIN
           @deletedInstances AS DI
           ON XQTE.Watermark = DI.Watermark;
    IF EXISTS (SELECT *
               FROM   @deletedTags)
        BEGIN
            DECLARE @deletedTagCounts AS TABLE (
                TagKey     BIGINT,
                ErrorCount INT   );
            INSERT INTO @deletedTagCounts (TagKey, ErrorCount)
            SELECT   TagKey,
                     COUNT(1)
            FROM     @deletedTags
            GROUP BY TagKey;
            UPDATE XQT
            SET    XQT.ErrorCount = XQT.ErrorCount - DTC.ErrorCount
            FROM   dbo.ExtendedQueryTag AS XQT
                   INNER JOIN
                   @deletedTagCounts AS DTC
                   ON XQT.TagKey = DTC.TagKey;
        END
    DELETE dbo.ExtendedQueryTagString
    WHERE  SopInstanceKey1 = @studyKey
           AND SopInstanceKey2 = ISNULL(@seriesKey, SopInstanceKey2)
           AND SopInstanceKey3 = ISNULL(@instanceKey, SopInstanceKey3)
           AND PartitionKey = @partitionKey
           AND ResourceType = @imageResourceType;
    DELETE dbo.ExtendedQueryTagLong
    WHERE  SopInstanceKey1 = @studyKey
           AND SopInstanceKey2 = ISNULL(@seriesKey, SopInstanceKey2)
           AND SopInstanceKey3 = ISNULL(@instanceKey, SopInstanceKey3)
           AND PartitionKey = @partitionKey
           AND ResourceType = @imageResourceType;
    DELETE dbo.ExtendedQueryTagDouble
    WHERE  SopInstanceKey1 = @studyKey
           AND SopInstanceKey2 = ISNULL(@seriesKey, SopInstanceKey2)
           AND SopInstanceKey3 = ISNULL(@instanceKey, SopInstanceKey3)
           AND PartitionKey = @partitionKey
           AND ResourceType = @imageResourceType;
    DELETE dbo.ExtendedQueryTagDateTime
    WHERE  SopInstanceKey1 = @studyKey
           AND SopInstanceKey2 = ISNULL(@seriesKey, SopInstanceKey2)
           AND SopInstanceKey3 = ISNULL(@instanceKey, SopInstanceKey3)
           AND PartitionKey = @partitionKey
           AND ResourceType = @imageResourceType;
    DELETE dbo.ExtendedQueryTagPersonName
    WHERE  SopInstanceKey1 = @studyKey
           AND SopInstanceKey2 = ISNULL(@seriesKey, SopInstanceKey2)
           AND SopInstanceKey3 = ISNULL(@instanceKey, SopInstanceKey3)
           AND PartitionKey = @partitionKey
           AND ResourceType = @imageResourceType;
    INSERT INTO dbo.ChangeFeed (Action, PartitionKey, StudyInstanceUid, SeriesInstanceUid, SopInstanceUid, OriginalWatermark, FilePath)
    SELECT 1,
           DI.PartitionKey,
           DI.StudyInstanceUid,
           DI.SeriesInstanceUid,
           DI.SopInstanceUid,
           DI.Watermark,
           FP.FilePath
    FROM   @deletedInstances AS DI
           LEFT OUTER JOIN
           dbo.FileProperty AS FP
           ON FP.Watermark = DI.Watermark
    WHERE  Status = @createdStatus;
    DELETE FP
    FROM   dbo.FileProperty AS FP
           INNER JOIN
           @deletedInstances AS DI
           ON FP.Watermark = DI.Watermark;
    INSERT INTO dbo.DeletedInstance (PartitionKey, StudyInstanceUid, SeriesInstanceUid, SopInstanceUid, Watermark, DeletedDateTime, RetryCount, CleanupAfter, OriginalWatermark)
    SELECT PartitionKey,
           StudyInstanceUid,
           SeriesInstanceUid,
           SopInstanceUid,
           Watermark,
           @deletedDate,
           0,
           @cleanupAfter,
           OriginalWatermark
    FROM   @deletedInstances;
    IF NOT EXISTS (SELECT *
                   FROM   dbo.Instance WITH (HOLDLOCK, UPDLOCK)
                   WHERE  PartitionKey = @partitionKey
                          AND StudyKey = @studyKey
                          AND SeriesKey = ISNULL(@seriesKey, SeriesKey))
        BEGIN
            DELETE dbo.Series
            WHERE  StudyKey = @studyKey
                   AND SeriesInstanceUid = ISNULL(@seriesInstanceUid, SeriesInstanceUid)
                   AND PartitionKey = @partitionKey;
            DELETE dbo.ExtendedQueryTagString
            WHERE  SopInstanceKey1 = @studyKey
                   AND SopInstanceKey2 = ISNULL(@seriesKey, SopInstanceKey2)
                   AND PartitionKey = @partitionKey
                   AND ResourceType = @imageResourceType;
            DELETE dbo.ExtendedQueryTagLong
            WHERE  SopInstanceKey1 = @studyKey
                   AND SopInstanceKey2 = ISNULL(@seriesKey, SopInstanceKey2)
                   AND PartitionKey = @partitionKey
                   AND ResourceType = @imageResourceType;
            DELETE dbo.ExtendedQueryTagDouble
            WHERE  SopInstanceKey1 = @studyKey
                   AND SopInstanceKey2 = ISNULL(@seriesKey, SopInstanceKey2)
                   AND PartitionKey = @partitionKey
                   AND ResourceType = @imageResourceType;
            DELETE dbo.ExtendedQueryTagDateTime
            WHERE  SopInstanceKey1 = @studyKey
                   AND SopInstanceKey2 = ISNULL(@seriesKey, SopInstanceKey2)
                   AND PartitionKey = @partitionKey
                   AND ResourceType = @imageResourceType;
            DELETE dbo.ExtendedQueryTagPersonName
            WHERE  SopInstanceKey1 = @studyKey
                   AND SopInstanceKey2 = ISNULL(@seriesKey, SopInstanceKey2)
                   AND PartitionKey = @partitionKey
                   AND ResourceType = @imageResourceType;
        END
    IF NOT EXISTS (SELECT *
                   FROM   dbo.Series WITH (HOLDLOCK, UPDLOCK)
                   WHERE  Studykey = @studyKey
                          AND PartitionKey = @partitionKey)
        BEGIN
            DELETE dbo.Study
            WHERE  StudyKey = @studyKey
                   AND PartitionKey = @partitionKey;
            DELETE dbo.ExtendedQueryTagString
            WHERE  SopInstanceKey1 = @studyKey
                   AND PartitionKey = @partitionKey
                   AND ResourceType = @imageResourceType;
            DELETE dbo.ExtendedQueryTagLong
            WHERE  SopInstanceKey1 = @studyKey
                   AND PartitionKey = @partitionKey
                   AND ResourceType = @imageResourceType;
            DELETE dbo.ExtendedQueryTagDouble
            WHERE  SopInstanceKey1 = @studyKey
                   AND PartitionKey = @partitionKey
                   AND ResourceType = @imageResourceType;
            DELETE dbo.ExtendedQueryTagDateTime
            WHERE  SopInstanceKey1 = @studyKey
                   AND PartitionKey = @partitionKey
                   AND ResourceType = @imageResourceType;
            DELETE dbo.ExtendedQueryTagPersonName
            WHERE  SopInstanceKey1 = @studyKey
                   AND PartitionKey = @partitionKey
                   AND ResourceType = @imageResourceType;
        END
    UPDATE CF
    SET    CF.CurrentWatermark = NULL
    FROM   dbo.ChangeFeed AS CF WITH (FORCESEEK)
           INNER JOIN
           @deletedInstances AS DI
           ON CF.PartitionKey = DI.PartitionKey
              AND CF.StudyInstanceUid = DI.StudyInstanceUid
              AND CF.SeriesInstanceUid = DI.SeriesInstanceUid
              AND CF.SopInstanceUid = DI.SopInstanceUid;
    COMMIT TRANSACTION;
END
GO

COMMIT TRANSACTION