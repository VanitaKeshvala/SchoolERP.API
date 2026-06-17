USE [SchoolERP]
GO

CREATE TABLE tbl_Library_Books (
    BookID INT PRIMARY KEY IDENTITY(1,1),
    BookTitle NVARCHAR(200) NOT NULL,
    BookNo NVARCHAR(50),
    ISBNNo NVARCHAR(50),
    Publisher NVARCHAR(200),
    Author NVARCHAR(200),
    Subject NVARCHAR(200),
    RackNo NVARCHAR(50),
    TotalQty INT DEFAULT 0,
    AvailableQty INT DEFAULT 0,
    BookPrice DECIMAL(18,2),
    PostDate DATETIME,
    Description NVARCHAR(1000),
    CompanyID INT,
    IsActive BIT DEFAULT 1,
    IsDelete BIT DEFAULT 0,
    CreatedOn DATETIME DEFAULT GETDATE(),
    CreatedBy INT,
    ModifiedOn DATETIME,
    ModifiedBy INT
);
GO
