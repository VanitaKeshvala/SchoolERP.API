USE [SchoolERP]
GO

CREATE TABLE tbl_Download_VideoTutorials (
    VideoID INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(200) NOT NULL,
    VideoLink NVARCHAR(500) NOT NULL,
    Description NVARCHAR(1000),
    ClassID INT,
    SectionID INT,
    CompanyID INT,
    IsActive BIT DEFAULT 1,
    IsDelete BIT DEFAULT 0,
    CreatedOn DATETIME DEFAULT GETDATE(),
    CreatedBy INT,
    ModifiedOn DATETIME,
    ModifiedBy INT
);
GO
