USE [SchoolERP]
GO

CREATE TABLE tbl_Download_SharedLinks (
    SharedLinkID INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(200) NOT NULL,
    ShareToken UNIQUEIDENTIFIER DEFAULT NEWID(),
    ShareDate DATETIME,
    ValidUpto DATETIME,
    CompanyID INT,
    IsDelete BIT DEFAULT 0,
    CreatedOn DATETIME DEFAULT GETDATE(),
    CreatedBy INT
);

CREATE TABLE tbl_Download_SharedLinkDetails (
    SharedLinkDetailID INT PRIMARY KEY IDENTITY(1,1),
    SharedLinkID INT,
    ContentID INT,
    FOREIGN KEY (SharedLinkID) REFERENCES tbl_Download_SharedLinks(SharedLinkID),
    FOREIGN KEY (ContentID) REFERENCES tbl_Download_Contents(ContentID)
);
GO
