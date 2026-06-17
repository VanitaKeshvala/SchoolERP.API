-- 1. Add columns to TBL_MST_STUDENTS if they don't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TBL_MST_STUDENTS') AND name = 'IsActive')
BEGIN
    ALTER TABLE TBL_MST_STUDENTS ADD IsActive BIT DEFAULT 1;
END
GO

-- Update existing records to be active
UPDATE TBL_MST_STUDENTS SET IsActive = 1 WHERE IsActive IS NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TBL_MST_STUDENTS') AND name = 'DisableReasonID')
BEGIN
    ALTER TABLE TBL_MST_STUDENTS ADD DisableReasonID INT NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TBL_MST_STUDENTS') AND name = 'DisableDate')
BEGIN
    ALTER TABLE TBL_MST_STUDENTS ADD DisableDate DATETIME NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TBL_MST_STUDENTS') AND name = 'DisableNote')
BEGIN
    ALTER TABLE TBL_MST_STUDENTS ADD DisableNote NVARCHAR(MAX) NULL;
END
GO

-- 2. Create/Update the toggle status procedure
CREATE OR ALTER PROCEDURE sp_Student_ToggleStatus
    @StudentID INT,
    @IsActive BIT,
    @DisableReasonID INT = NULL,
    @DisableDate DATETIME = NULL,
    @DisableNote NVARCHAR(MAX) = NULL,
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        -- Update Student Master
        UPDATE TBL_MST_STUDENTS SET
            IsActive = @IsActive,
            DisableReasonID = @DisableReasonID,
            DisableDate = @DisableDate,
            DisableNote = @DisableNote,
            MODIFIEDON = GETDATE(),
            MODIFIEDBY = @UserID
        WHERE STUDENTID = @StudentID;

        -- Also update the associated Student User if exists
        DECLARE @StudentUserID INT = (SELECT STUDENTUSERID FROM TBL_MST_STUDENTS WHERE STUDENTID = @StudentID);
        IF @StudentUserID IS NOT NULL
        BEGIN
            UPDATE TBL_MST_USERS SET ISACTIVE = @IsActive WHERE USERID = @StudentUserID;
        END

        COMMIT TRANSACTION;
        SELECT 1 AS RESULT, 'Student status updated successfully' AS MESSAGE;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SELECT 0 AS RESULT, ERROR_MESSAGE() AS MESSAGE;
    END CATCH
END
GO
