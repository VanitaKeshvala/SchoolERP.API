USE [SchoolERP]
GO

CREATE TABLE tbl_Download_Contents (
    ContentID INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(200) NOT NULL,
    ContentTypeID INT,
    FileType NVARCHAR(20), -- 'File' or 'Link'
    FileName NVARCHAR(500), -- Original/Physical file name
    FilePath NVARCHAR(1000), -- Relative path on server or YouTube URL
    FileSize NVARCHAR(50), -- e.g., '2.5 MB' or 'N/A'
    CompanyID INT,
    IsActive BIT DEFAULT 1,
    IsDelete BIT DEFAULT 0,
    CreatedOn DATETIME DEFAULT GETDATE(),
    CreatedBy INT,
    ModifiedOn DATETIME,
    ModifiedBy INT
);
GO
