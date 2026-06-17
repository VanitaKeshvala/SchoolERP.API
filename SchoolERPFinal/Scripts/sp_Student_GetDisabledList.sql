USE [SchoolERP]
GO

-- ======================================================
-- Procedure: sp_Student_GetDisabledList
-- Description: Gets students where IsActive = 0
-- ======================================================
CREATE OR ALTER PROCEDURE sp_Student_GetDisabledList
    @COMPANYID INT,
    @SESSIONID INT,
    @CLASSID INT = NULL,
    @SECTIONID INT = NULL,
    @SEARCHTERM NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        s.STUDENTID, 
        s.ROLLNO, 
        s.ADMISSIONNO, 
        s.FIRSTNAME, 
        s.MIDDLENAME, 
        s.LASTNAME,
        s.GENDER, 
        s.DOB, 
        s.MOBILENO, 
        s.FATHERNAME, 
        s.FATHERPHONE,
        s.STUDENTPHOTO, 
        s.STUDENTPHOTOTYPE,
        c.ClassName, 
        sec.SectionName, 
        cat.StudentCategoryName, -- Corrected column name
        s.IsActive, 
        s.DisableReasonID, 
        dr.ReasonName AS DisableReasonName,
        s.DisableDate, 
        s.DisableNote
    FROM TBL_MST_STUDENTS s
    LEFT JOIN TBL_MST_CLASSES c ON s.CLASSID = c.ClassID
    LEFT JOIN TBL_MST_SECTIONS sec ON s.SECTIONID = sec.SectionID
    LEFT JOIN tbl_Student_DisableReasons dr ON s.DisableReasonID = dr.DisableReasonID
    LEFT JOIN TBL_MST_STUDENT_CATEGORIES cat ON s.CATEGORYID = cat.StudentCategoryID -- Corrected table/column
    WHERE s.COMPANYID = @COMPANYID 
      AND s.SESSIONID = @SESSIONID
      AND s.ISACTIVE = 0
      AND s.ISDELETE = 0
      AND (@CLASSID IS NULL OR s.CLASSID = @CLASSID)
      AND (@SECTIONID IS NULL OR s.SECTIONID = @SECTIONID)
      AND (@SEARCHTERM IS NULL OR 
           (s.FIRSTNAME + ' ' + ISNULL(s.MIDDLENAME, '') + ' ' + s.LASTNAME) LIKE '%' + @SEARCHTERM + '%' OR 
           s.ROLLNO LIKE '%' + @SEARCHTERM + '%' OR
           s.ADMISSIONNO LIKE '%' + @SEARCHTERM + '%'
          );
END
GO
