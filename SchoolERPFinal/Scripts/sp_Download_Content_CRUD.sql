USE [SchoolERP]
GO

-- ======================================================
-- Procedure: sp_Download_Content_CRUD
-- Description: Handles Download Content operations
-- ======================================================
CREATE OR ALTER PROCEDURE sp_Download_Content_CRUD
    @Action NVARCHAR(20),
    @ContentID INT = NULL,
    @Title NVARCHAR(200) = NULL,
    @ContentTypeID INT = NULL,
    @FileType NVARCHAR(20) = NULL,
    @FileName NVARCHAR(500) = NULL,
    @FilePath NVARCHAR(1000) = NULL,
    @FileSize NVARCHAR(50) = NULL,
    @CompanyID INT = NULL,
    @UserID INT = NULL,
    @SearchTerm NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Action = 'LIST'
    BEGIN
        SELECT 
            c.ContentID, 
            c.Title, 
            c.FileType, 
            c.FileName, 
            c.FilePath, 
            c.FileSize, 
            ct.TypeName AS ContentTypeName,
            stf.Name AS UploadBy,
            c.CreatedOn,
            c.IsActive
        FROM tbl_Download_Contents c
        LEFT JOIN tbl_Download_ContentTypes ct ON c.ContentTypeID = ct.ContentTypeID
        LEFT JOIN tbl_Staff_Personal_Details stf ON c.CreatedBy = stf.UserID
        WHERE c.CompanyID = @CompanyID AND c.IsDelete = 0
          AND (@SearchTerm IS NULL OR c.Title LIKE '%' + @SearchTerm + '%')
        ORDER BY c.CreatedOn DESC;
    END

    ELSE IF @Action = 'SAVE'
    BEGIN
        IF @ContentID IS NULL OR @ContentID = 0
        BEGIN
            INSERT INTO tbl_Download_Contents (Title, ContentTypeID, FileType, FileName, FilePath, FileSize, CompanyID, CreatedBy, CreatedOn)
            VALUES (@Title, @ContentTypeID, @FileType, @FileName, @FilePath, @FileSize, @CompanyID, @UserID, GETDATE());
            SELECT 1 AS Result, 'Content uploaded successfully' AS Message;
        END
        ELSE
        BEGIN
            UPDATE tbl_Download_Contents
            SET Title = @Title, ContentTypeID = @ContentTypeID, ModifiedBy = @UserID, ModifiedOn = GETDATE()
            WHERE ContentID = @ContentID;
            SELECT 1 AS Result, 'Content updated successfully' AS Message;
        END
    END

    ELSE IF @Action = 'DELETE'
    BEGIN
        UPDATE tbl_Download_Contents SET IsDelete = 1, ModifiedBy = @UserID, ModifiedOn = GETDATE() WHERE ContentID = @ContentID;
        SELECT 1 AS Result, 'Content deleted successfully' AS Message;
    END
END
GO
