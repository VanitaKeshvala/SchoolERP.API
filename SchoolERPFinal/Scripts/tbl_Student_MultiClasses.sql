-- ======================================================
-- Table: tbl_Student_MultiClasses
-- Description: Stores additional class/section assignments for students
-- ======================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tbl_Student_MultiClasses')
BEGIN
    CREATE TABLE tbl_Student_MultiClasses (
        MultiClassID INT PRIMARY KEY IDENTITY(1,1),
        StudentID INT NOT NULL,
        ClassID INT NOT NULL,
        SectionID INT NOT NULL,
        CompanyID INT NOT NULL,
        SessionID INT NOT NULL,
        CreatedOn DATETIME DEFAULT GETDATE(),
        CreatedBy INT,
        ModifiedOn DATETIME,
        ModifiedBy INT,
        IsDelete BIT DEFAULT 0,
        CONSTRAINT FK_MultiClass_Student FOREIGN KEY (StudentID) REFERENCES TBL_MST_STUDENTS(STUDENTID)
    );
END
GO

-- ======================================================
-- Procedure: sp_Student_MultiClasses_Get
-- Description: Gets multi-class assignments for a student or by filter
-- ======================================================
CREATE OR ALTER PROCEDURE sp_Student_MultiClasses_Get
    @StudentID INT = NULL,
    @CompanyID INT,
    @SessionID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        mc.*,
        c.ClassName,
        s.SectionName
    FROM tbl_Student_MultiClasses mc
    JOIN TBL_MST_CLASSES c ON mc.ClassID = c.ClassID
    JOIN TBL_MST_SECTIONS s ON mc.SectionID = s.SectionID
    WHERE mc.CompanyID = @CompanyID 
      AND mc.SessionID = @SessionID
      AND mc.IsDelete = 0
      AND (@StudentID IS NULL OR mc.StudentID = @StudentID);
END
GO

-- ======================================================
-- Procedure: sp_Student_MultiClasses_Upsert
-- Description: Adds or updates a multi-class assignment
-- ======================================================
CREATE OR ALTER PROCEDURE sp_Student_MultiClasses_Upsert
    @MultiClassID INT,
    @StudentID INT,
    @ClassID INT,
    @SectionID INT,
    @CompanyID INT,
    @SessionID INT,
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check if assignment already exists
    IF EXISTS (SELECT 1 FROM tbl_Student_MultiClasses 
               WHERE StudentID = @StudentID AND ClassID = @ClassID AND SectionID = @SectionID 
               AND MultiClassID <> @MultiClassID AND CompanyID = @CompanyID AND SessionID = @SessionID
               AND IsDelete = 0)
    BEGIN
        SELECT 0 AS Result, 'This class assignment already exists for the student.' AS Message;
        RETURN;
    END

    IF @MultiClassID > 0
    BEGIN
        UPDATE tbl_Student_MultiClasses SET
            ClassID = @ClassID,
            SectionID = @SectionID,
            ModifiedOn = GETDATE(),
            ModifiedBy = @UserID
        WHERE MultiClassID = @MultiClassID;
        
        SELECT 1 AS Result, 'Assignment updated successfully' AS Message;
    END
    ELSE
    BEGIN
        INSERT INTO tbl_Student_MultiClasses (StudentID, ClassID, SectionID, CompanyID, SessionID, CreatedOn, CreatedBy)
        VALUES (@StudentID, @ClassID, @SectionID, @CompanyID, @SessionID, GETDATE(), @UserID);
        
        SELECT 1 AS Result, 'Assignment added successfully' AS Message;
    END
END
GO

-- ======================================================
-- Procedure: sp_Student_MultiClasses_Delete
-- Description: Deletes a multi-class assignment
-- ======================================================
CREATE OR ALTER PROCEDURE sp_Student_MultiClasses_Delete
    @MultiClassID INT,
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tbl_Student_MultiClasses SET IsDelete = 1, ModifiedBy = @UserID, ModifiedOn = GETDATE() 
    WHERE MultiClassID = @MultiClassID;
    SELECT 1 AS Result, 'Assignment removed successfully' AS Message;
END
GO

-- ======================================================
-- Procedure: sp_Student_MultiClasses_SearchStudents
-- Description: Searches students with their multi-class assignments
-- ======================================================
CREATE OR ALTER PROCEDURE sp_Student_MultiClasses_SearchStudents
    @ClassID INT = NULL,
    @SectionID INT = NULL,
    @SearchTerm NVARCHAR(100) = NULL,
    @CompanyID INT,
    @SessionID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Select students based on primary enrollment
    SELECT 
        s.STUDENTID,
        s.ROLLNO,
        s.FIRSTNAME + ' ' + ISNULL(s.MIDDLENAME, '') + ' ' + s.LASTNAME AS FullName,
        s.CLASSID AS PrimaryClassID,
        c.ClassName AS PrimaryClassName,
        s.SECTIONID AS PrimarySectionID,
        sec.SectionName AS PrimarySectionName
    FROM TBL_MST_STUDENTS s
    JOIN TBL_MST_CLASSES c ON s.CLASSID = c.ClassID
    JOIN TBL_MST_SECTIONS sec ON s.SECTIONID = sec.SectionID
    WHERE s.COMPANYID = @CompanyID
      AND (@ClassID IS NULL OR s.CLASSID = @ClassID)
      AND (@SectionID IS NULL OR s.SECTIONID = @SectionID)
      AND (@SearchTerm IS NULL OR (s.FIRSTNAME + ' ' + s.LASTNAME) LIKE '%' + @SearchTerm + '%' OR s.ROLLNO LIKE '%' + @SearchTerm + '%')
      AND s.ISACTIVE = 1 AND s.ISDELETE = 0;
END
GO
