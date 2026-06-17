USE [SchoolERP]
GO

-- ======================================================
-- Procedure: sp_Student_BulkDelete
-- Description: Soft deletes multiple students at once
-- ======================================================
CREATE OR ALTER PROCEDURE sp_Student_BulkDelete
    @StudentIDs NVARCHAR(MAX), -- Comma separated IDs
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        -- Update Student Master
        UPDATE TBL_MST_STUDENTS 
        SET IsDelete = 1, MODIFIEDON = GETDATE(), MODIFIEDBY = @UserID
        WHERE STUDENTID IN (SELECT CAST(value AS INT) FROM STRING_SPLIT(@StudentIDs, ','));
        
        -- Update associated Student Users
        UPDATE TBL_MST_USERS
        SET ISDELETE = 1, MODIFIEDON = GETDATE(), MODIFIEDBY = @UserID
        WHERE USERID IN (
            SELECT STUDENTUSERID 
            FROM TBL_MST_STUDENTS 
            WHERE STUDENTID IN (SELECT CAST(value AS INT) FROM STRING_SPLIT(@StudentIDs, ','))
        );

        COMMIT TRANSACTION;
        SELECT 1 AS Result, 'Selected students have been deleted successfully' AS Message;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SELECT 0 AS Result, ERROR_MESSAGE() AS Message;
    END CATCH
END
GO
