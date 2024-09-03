CREATE PROCEDURE ProcessDataFromSingleLinkedServer
    @SourceDbName NVARCHAR(100),
    @LinkedServer NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @LastProcessedId INT;
    DECLARE @NewLastProcessedId INT;
    DECLARE @RowsProcessed INT;

    DECLARE @Sql NVARCHAR(MAX);
    DECLARE @ReceivedData TABLE (
        id INT,
        re_fromnum NVARCHAR(50),
        re_message NVARCHAR(MAX)
    );

    -- Get the last processed ID from the tracking table
    SELECT @LastProcessedId = LastProcessedId
    FROM ProcessedRecords
    WHERE SourceDbName = @SourceDbName;

    IF @LastProcessedId IS NULL
    BEGIN
        -- If no record exists, initialize @LastProcessedId to 0
        SET @LastProcessedId = 0;
    END

    SET @RowsProcessed = 1;

    WHILE @RowsProcessed > 0
    BEGIN
        -- Construct the SQL command to fetch data from the linked server
        -- SP on the source also an option
        SET @Sql = '
            SELECT TOP 1000 id, re_fromnum, re_message
            FROM ' + QUOTENAME(@LinkedServer) + '.' + QUOTENAME(@SourceDbName) + '.dbo.received
            WHERE status = 1 AND id > ' + CAST(@LastProcessedId AS NVARCHAR) + '
            ORDER BY id';

        -- Clear the temporary table before populating with new data
        DELETE FROM @ReceivedData;

        -- Execute the dynamic SQL and insert the data into the temporary table
        INSERT INTO @ReceivedData (id, re_fromnum, re_message)
        EXEC sp_executesql @Sql;

        -- Count the rows retrieved in this batch
        SET @RowsProcessed = @@ROWCOUNT;

        -- If data was retrieved, process it
        IF @RowsProcessed > 0
        BEGIN
            BEGIN TRANSACTION;

            -- Insert the data into the local processing table
            INSERT INTO received_total (rt_msisdn, rt_message)
            SELECT re_fromnum, re_message
            FROM @ReceivedData;

            -- Get the maximum ID from the processed batch to update the tracking table
            SELECT @NewLastProcessedId = MAX(id) FROM @ReceivedData;

            -- Update the tracking table with the new last processed ID
            UPDATE ProcessedRecords
            SET LastProcessedId = @NewLastProcessedId, LastProcessedAt = GETDATE()
            WHERE SourceDbName = @SourceDbName;

            -- If no record exists in the tracking table, insert it
            IF @@ROWCOUNT = 0
            BEGIN
                INSERT INTO ProcessedRecords (SourceDbName, LastProcessedId, LastProcessedAt)
                VALUES (@SourceDbName, @NewLastProcessedId, GETDATE());
            END

            -- Update @LastProcessedId for the next iteration
            SET @LastProcessedId = @NewLastProcessedId;

            COMMIT TRANSACTION;
        END
    END
END
