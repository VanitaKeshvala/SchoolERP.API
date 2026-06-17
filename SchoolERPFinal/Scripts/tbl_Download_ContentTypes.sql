USE [SchoolERP]
GO

CREATE TABLE tbl_Download_ContentTypes (
    ContentTypeID INT PRIMARY KEY IDENTITY(1,1),
    TypeName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    CompanyID INT,
    IsActive BIT DEFAULT 1,
    IsDelete BIT DEFAULT 0,
    CreatedOn DATETIME DEFAULT GETDATE(),
    CreatedBy INT,
    ModifiedOn DATETIME,
    ModifiedBy INT
);
GO
