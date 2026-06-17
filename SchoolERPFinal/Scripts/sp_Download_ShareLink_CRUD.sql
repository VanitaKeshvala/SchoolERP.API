USE [SchoolERP]
GO

-- ======================================================
-- Procedure: sp_Download_ShareLink_CRUD
-- Description: Handles Shared Link generation and retrieval
-- ======================================================
CREATE OR ALTER PROCEDURE sp_Download_ShareLink_CRUD
    @Action NVARCHAR(20),
    @SharedLinkID INT = NULL,
    @Title NVARCHAR(200) = NULL,
    @ShareDate DATETIME = NULL,
    @ValidUpto DATETIME = NULL,
    @ContentIDs NVARCHAR(MAX) = NULL, -- Comma separated IDs
    @CompanyID INT = NULL,
    @UserID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Action = 'SAVE'
    BEGIN
        DECLARE @NewSharedLinkID INT;
        DECLARE @Token UNIQUEIDENTIFIER = NEWID();

        INSERT INTO tbl_Download_SharedLinks (Title, ShareToken, ShareDate, ValidUpto, CompanyID, CreatedBy, CreatedOn)
        VALUES (@Title, @Token, @ShareDate, @ValidUpto, @CompanyID, @UserID, GETDATE());

        SET @NewSharedLinkID = SCOPE_IDENTITY();

        -- Insert details from comma separated string
        INSERT INTO tbl_Download_SharedLinkDetails (SharedLinkID, ContentID)
        SELECT @NewSharedLinkID, CAST(value AS INT)
        FROM STRING_SPLIT(@ContentIDs, ',');

        SELECT 1 AS Result, 'Shareable link generated successfully' AS Message, CAST(@Token AS NVARCHAR(50)) AS Extra;
    END

    ELSE IF @Action = 'LIST'
    BEGIN
        SELECT 
            sl.SharedLinkID, 
            sl.Title, 
            sl.ShareToken, 
            sl.ShareDate, 
            sl.ValidUpto, 
            sl.CreatedOn,
            stf.Name AS SharedBy
        FROM tbl_Download_SharedLinks sl
        LEFT JOIN tbl_Staff_Personal_Details stf ON sl.CreatedBy = stf.UserID
        WHERE sl.CompanyID = @CompanyID AND sl.IsDelete = 0
        ORDER BY sl.CreatedOn DESC;
    END

    ELSE IF @Action = 'DELETE'
    BEGIN
        UPDATE tbl_Download_SharedLinks SET IsDelete = 1 WHERE SharedLinkID = @SharedLinkID;
        SELECT 1 AS Result, 'Shared link deleted successfully' AS Message;
    END
END
GO
