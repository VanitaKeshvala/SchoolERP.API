USE [SchoolERP]
GO

-- ======================================================
-- Procedure: sp_Download_ContentType_CRUD
-- Description: Handles Content Type operations
-- ======================================================
CREATE OR ALTER PROCEDURE sp_Download_ContentType_CRUD
    @Action NVARCHAR(20),
    @ContentTypeID INT = NULL,
    @TypeName NVARCHAR(100) = NULL,
    @Description NVARCHAR(500) = NULL,
    @CompanyID INT = NULL,
    @UserID INT = NULL,
    @SearchTerm NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Action = 'LIST'
    BEGIN
        SELECT ContentTypeID, TypeName, Description, IsActive
        FROM tbl_Download_ContentTypes
        WHERE CompanyID = @CompanyID AND IsDelete = 0
          AND (@SearchTerm IS NULL OR TypeName LIKE '%' + @SearchTerm + '%')
        ORDER BY TypeName;
    END

    ELSE IF @Action = 'GETBYID'
    BEGIN
        SELECT ContentTypeID, TypeName, Description, IsActive
        FROM tbl_Download_ContentTypes
        WHERE ContentTypeID = @ContentTypeID;
    END

    ELSE IF @Action = 'SAVE'
    BEGIN
        IF EXISTS (SELECT 1 FROM tbl_Download_ContentTypes WHERE TypeName = @TypeName AND CompanyID = @CompanyID AND IsDelete = 0 AND (@ContentTypeID IS NULL OR ContentTypeID <> @ContentTypeID))
        BEGIN
            SELECT 0 AS Result, 'Content Type with this name already exists' AS Message;
            RETURN;
        END

        IF @ContentTypeID IS NULL OR @ContentTypeID = 0
        BEGIN
            INSERT INTO tbl_Download_ContentTypes (TypeName, Description, CompanyID, CreatedBy, CreatedOn)
            VALUES (@TypeName, @Description, @CompanyID, @UserID, GETDATE());
            SELECT 1 AS Result, 'Content Type saved successfully' AS Message;
        END
        ELSE
        BEGIN
            UPDATE tbl_Download_ContentTypes
            SET TypeName = @TypeName, Description = @Description, ModifiedBy = @UserID, ModifiedOn = GETDATE()
            WHERE ContentTypeID = @ContentTypeID;
            SELECT 1 AS Result, 'Content Type updated successfully' AS Message;
        END
    END

    ELSE IF @Action = 'DELETE'
    BEGIN
        UPDATE tbl_Download_ContentTypes SET IsDelete = 1, ModifiedBy = @UserID, ModifiedOn = GETDATE() WHERE ContentTypeID = @ContentTypeID;
        SELECT 1 AS Result, 'Content Type deleted successfully' AS Message;
    END

    ELSE IF @Action = 'TOGGLESTATUS'
    BEGIN
        UPDATE tbl_Download_ContentTypes SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END, ModifiedBy = @UserID, ModifiedOn = GETDATE() WHERE ContentTypeID = @ContentTypeID;
        SELECT 1 AS Result, 'Status updated successfully' AS Message;
    END
END
GO
