USE [SchoolERP]
GO

CREATE TABLE tbl_Library_Members (
    LibraryMemberID INT PRIMARY KEY IDENTITY(1,1),
    MemberType NVARCHAR(20) NOT NULL, -- 'Student' or 'Staff'
    StudentID INT NULL, -- Links to tbl_Student_Admission
    StaffID INT NULL, -- Links to tbl_Staff_Personal_Details
    LibraryCardNo NVARCHAR(50),
    CompanyID INT,
    IsActive BIT DEFAULT 1,
    IsDelete BIT DEFAULT 0,
    CreatedOn DATETIME DEFAULT GETDATE(),
    CreatedBy INT,
    ModifiedOn DATETIME,
    ModifiedBy INT
);
GO

-- Optional: Filtered Unique Index to ensure CardNo is unique when provided
-- CREATE UNIQUE INDEX IX_LibraryMembers_CardNo_Unique ON tbl_Library_Members(LibraryCardNo, CompanyID) WHERE LibraryCardNo IS NOT NULL AND IsDelete = 0;
GO
