USE [SchoolERP]
GO

-- ======================================================
-- Procedure: sp_Library_Member_CRUD
-- Description: Updated with Card Number uniqueness check
-- ======================================================
CREATE OR ALTER PROCEDURE sp_Library_Member_CRUD
    @Action NVARCHAR(20),
    @LibraryMemberID INT = NULL,
    @MemberType NVARCHAR(20) = NULL,
    @StudentID INT = NULL,
    @StaffID INT = NULL,
    @LibraryCardNo NVARCHAR(50) = NULL,
    @CompanyID INT = NULL,
    @ClassID INT = NULL,
    @SectionID INT = NULL,
    @DepartmentID INT = NULL,
    @UserID INT = NULL,
    @SearchTerm NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Action = 'LIST'
    BEGIN
        IF @MemberType = 'Student'
        BEGIN
            SELECT 
                s.StudentID,
                s.AdmissionNo,
                s.FirstName + ' ' + ISNULL(s.LastName, '') AS StudentName,
                c.ClassName + '(' + sec.SectionName + ')' AS ClassName,
                s.FatherName,
                s.Gender,
                s.MobileNo,
                ISNULL(lm.LibraryMemberID, 0) AS LibraryMemberID,
                lm.LibraryCardNo,
                lm.CreatedOn AS RegisteredOn
            FROM tbl_Student_Admission s
            LEFT JOIN tbl_Library_Members lm ON s.StudentID = lm.StudentID AND lm.IsDelete = 0 AND lm.MemberType = 'Student'
            LEFT JOIN tbl_Class c ON s.ClassID = c.ClassID
            LEFT JOIN tbl_Section sec ON s.SectionID = sec.SectionID
            WHERE s.CompanyID = @CompanyID AND s.IsDelete = 0
              AND (@ClassID IS NULL OR s.ClassID = @ClassID)
              AND (@SectionID IS NULL OR s.SectionID = @SectionID)
              AND (@SearchTerm IS NULL OR s.FirstName LIKE '%' + @SearchTerm + '%' OR s.AdmissionNo LIKE '%' + @SearchTerm + '%')
            ORDER BY s.FirstName;
        END
        ELSE IF @MemberType = 'Staff'
        BEGIN
            SELECT 
                stf.StaffID,
                stf.StaffCode AS AdmissionNo,
                stf.FirstName + ' ' + ISNULL(stf.LastName, '') AS StudentName,
                dept.DepartmentName AS ClassName,
                stf.Gender,
                stf.MobileNo,
                ISNULL(lm.LibraryMemberID, 0) AS LibraryMemberID,
                lm.LibraryCardNo,
                lm.CreatedOn AS RegisteredOn
            FROM tbl_Staff_Personal_Details stf
            LEFT JOIN tbl_Library_Members lm ON stf.StaffID = lm.StaffID AND lm.IsDelete = 0 AND lm.MemberType = 'Staff'
            LEFT JOIN tbl_Mst_Department dept ON stf.DepartmentID = dept.DepartmentID
            WHERE stf.CompanyID = @CompanyID AND stf.IsDelete = 0
              AND (@DepartmentID IS NULL OR stf.DepartmentID = @DepartmentID)
              AND (@SearchTerm IS NULL OR stf.FirstName LIKE '%' + @SearchTerm + '%' OR stf.StaffCode LIKE '%' + @SearchTerm + '%')
            ORDER BY stf.FirstName;
        END
    END

    ELSE IF @Action = 'SAVE'
    BEGIN
        -- 1. Check if student/staff is already a library member
        IF EXISTS (SELECT 1 FROM tbl_Library_Members WHERE MemberType = @MemberType AND CompanyID = @CompanyID AND IsDelete = 0 AND (StudentID = @StudentID OR StaffID = @StaffID))
        BEGIN
            SELECT 0 AS Result, 'Member already registered in Library' AS Message;
            RETURN;
        END

        -- 2. Check for unique Library Card Number (if provided)
        IF ISNULL(@LibraryCardNo, '') <> ''
        BEGIN
            IF EXISTS (SELECT 1 FROM tbl_Library_Members WHERE LibraryCardNo = @LibraryCardNo AND CompanyID = @CompanyID AND IsDelete = 0)
            BEGIN
                SELECT 0 AS Result, 'Library Card Number already in use' AS Message;
                RETURN;
            END
        END

        INSERT INTO tbl_Library_Members (MemberType, StudentID, StaffID, LibraryCardNo, CompanyID, CreatedBy, CreatedOn)
        VALUES (@MemberType, @StudentID, @StaffID, @LibraryCardNo, @CompanyID, @UserID, GETDATE());
        
        SELECT 1 AS Result, 'Member registered successfully' AS Message;
    END

    ELSE IF @Action = 'DELETE'
    BEGIN
        IF @StudentID IS NOT NULL
            UPDATE tbl_Library_Members SET IsDelete = 1, ModifiedBy = @UserID, ModifiedOn = GETDATE() WHERE StudentID = @StudentID AND MemberType = 'Student';
        ELSE IF @StaffID IS NOT NULL
            UPDATE tbl_Library_Members SET IsDelete = 1, ModifiedBy = @UserID, ModifiedOn = GETDATE() WHERE StaffID = @StaffID AND MemberType = 'Staff';
        ELSE
            UPDATE tbl_Library_Members SET IsDelete = 1, ModifiedBy = @UserID, ModifiedOn = GETDATE() WHERE LibraryMemberID = @LibraryMemberID;
            
        SELECT 1 AS Result, 'Membership cancelled successfully' AS Message;
    END
END
GO
