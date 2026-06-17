USE [SchoolERP]
GO

-- ======================================================
-- Procedure: sp_Download_VideoTutorial_CRUD
-- Description: Handles Video Tutorial operations
-- ======================================================
CREATE OR ALTER PROCEDURE sp_Download_VideoTutorial_CRUD
    @Action NVARCHAR(20),
    @VideoID INT = NULL,
    @Title NVARCHAR(200) = NULL,
    @VideoLink NVARCHAR(500) = NULL,
    @Description NVARCHAR(1000) = NULL,
    @ClassID INT = NULL,
    @SectionID INT = NULL,
    @CompanyID INT = NULL,
    @UserID INT = NULL,
    @SearchTerm NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Action = 'LIST'
    BEGIN
        SELECT 
            v.VideoID, 
            v.Title, 
            v.VideoLink, 
            v.Description, 
            v.ClassID, 
            v.SectionID, 
            c.ClassName, 
            s.SectionName,
            v.CreatedOn,
            stf.Name AS SharedBy,
            v.IsActive
        FROM tbl_Download_VideoTutorials v
        LEFT JOIN MstClass c ON v.ClassID = c.ClassID
        LEFT JOIN MstSection s ON v.SectionID = s.SectionID
        LEFT JOIN tbl_Staff_Personal_Details stf ON v.CreatedBy = stf.UserID
        WHERE v.CompanyID = @CompanyID AND v.IsDelete = 0
          AND (@ClassID IS NULL OR v.ClassID = @ClassID)
          AND (@SectionID IS NULL OR v.SectionID = @SectionID)
          AND (@SearchTerm IS NULL OR v.Title LIKE '%' + @SearchTerm + '%')
        ORDER BY v.CreatedOn DESC;
    END

    ELSE IF @Action = 'GETBYID'
    BEGIN
        SELECT VideoID, Title, VideoLink, Description, ClassID, SectionID, IsActive
        FROM tbl_Download_VideoTutorials
        WHERE VideoID = @VideoID;
    END

    ELSE IF @Action = 'SAVE'
    BEGIN
        IF @VideoID IS NULL OR @VideoID = 0
        BEGIN
            INSERT INTO tbl_Download_VideoTutorials (Title, VideoLink, Description, ClassID, SectionID, CompanyID, CreatedBy, CreatedOn)
            VALUES (@Title, @VideoLink, @Description, @ClassID, @SectionID, @CompanyID, @UserID, GETDATE());
            SELECT 1 AS Result, 'Video Tutorial shared successfully' AS Message;
        END
        ELSE
        BEGIN
            UPDATE tbl_Download_VideoTutorials
            SET Title = @Title, 
                VideoLink = @VideoLink, 
                Description = @Description, 
                ClassID = @ClassID, 
                SectionID = @SectionID, 
                ModifiedBy = @UserID, 
                ModifiedOn = GETDATE()
            WHERE VideoID = @VideoID;
            SELECT 1 AS Result, 'Video Tutorial updated successfully' AS Message;
        END
    END

    ELSE IF @Action = 'DELETE'
    BEGIN
        UPDATE tbl_Download_VideoTutorials SET IsDelete = 1, ModifiedBy = @UserID, ModifiedOn = GETDATE() WHERE VideoID = @VideoID;
        SELECT 1 AS Result, 'Video Tutorial deleted successfully' AS Message;
    END

    ELSE IF @Action = 'TOGGLESTATUS'
    BEGIN
        UPDATE tbl_Download_VideoTutorials SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END, ModifiedBy = @UserID, ModifiedOn = GETDATE() WHERE VideoID = @VideoID;
        SELECT 1 AS Result, 'Status updated successfully' AS Message;
    END
END
GO
