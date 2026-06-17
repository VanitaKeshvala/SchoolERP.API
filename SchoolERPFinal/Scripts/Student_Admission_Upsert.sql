-- =============================================
-- Table: tbl_StudentAdmission (Optional - if it doesn't exist)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tbl_StudentAdmission')
BEGIN
    CREATE TABLE tbl_StudentAdmission (
        StudentID INT PRIMARY KEY IDENTITY(1,1),
        RollNo NVARCHAR(50),
        CompanyID INT,
        SessionID INT,
        AdmissionData NVARCHAR(MAX), -- Stores all dynamic fields as JSON
        IsActive BIT DEFAULT 1,
        CreatedOn DATETIME DEFAULT GETDATE(),
        CreatedBy INT,
        ModifiedOn DATETIME,
        ModifiedBy INT
    );
END
GO

-- =============================================
-- Procedure: sp_Student_Admission_Upsert
-- Description: Saves or updates a student admission record.
-- =============================================
CREATE OR ALTER PROCEDURE sp_Student_Admission_Upsert
    @StudentID INT,
    @RollNo NVARCHAR(50),
    @FieldValues NVARCHAR(MAX), -- JSON string containing ALL form fields
    @CompanyID INT,
    @SessionID INT,
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF @StudentID > 0
        BEGIN
            -- Update existing student
            UPDATE tbl_StudentAdmission
            SET RollNo = @RollNo,
                AdmissionData = @FieldValues,
                ModifiedOn = GETDATE(),
                ModifiedBy = @UserID
            WHERE StudentID = @StudentID;

            SELECT 1 AS Result, 'Student admission updated successfully' AS Message, @StudentID AS StudentID;
        END
        ELSE
        BEGIN
            -- Insert new student
            INSERT INTO tbl_StudentAdmission (RollNo, CompanyID, SessionID, AdmissionData, CreatedBy)
            VALUES (@RollNo, @CompanyID, @SessionID, @FieldValues, @UserID);

            SELECT 1 AS Result, 'Student admission saved successfully' AS Message, SCOPE_IDENTITY() AS StudentID;
        END
    END TRY
    BEGIN CATCH
        SELECT 0 AS Result, ERROR_MESSAGE() AS Message, 0 AS StudentID;
    END CATCH
END
GO
