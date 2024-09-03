CREATE PROCEDURE usp_GetReceivedData
    @LastProcessedId INT,
    @BatchSize INT
AS
BEGIN
    SELECT id, re_fromnum, re_message
    FROM received --With (NOLOCK)
    WHERE status = 1 AND id > @LastProcessedId
    ORDER BY id
    FETCH NEXT @BatchSize ROWS ONLY;
END