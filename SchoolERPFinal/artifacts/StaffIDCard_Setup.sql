-- Table: tbl_StaffIDCard
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tbl_StaffIDCard]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[tbl_StaffIDCard](
    [IDCardID] [int] IDENTITY(1,1) NOT NULL,
    [IDCardTitle] [nvarchar](200) NOT NULL,
    [SchoolName] [nvarchar](200) NULL,
    [HeaderColor] [nvarchar](50) NULL,
    [AddressPhoneEmail] [nvarchar](max) NULL,
    
    -- Images
    [BackgroundImage] [varbinary](max) NULL,
    [BackgroundImageType] [nvarchar](50) NULL,
    [BackgroundImageName] [nvarchar](200) NULL,
    
    [LogoImage] [varbinary](max) NULL,
    [LogoImageType] [nvarchar](50) NULL,
    [LogoImageName] [nvarchar](200) NULL,
    
    [SignatureImage] [varbinary](max) NULL,
    [SignatureImageType] [nvarchar](50) NULL,
    [SignatureImageName] [nvarchar](200) NULL,
    
    -- Toggles
    [ShowStaffName] [bit] NOT NULL DEFAULT 1,
    [ShowDesignation] [bit] NOT NULL DEFAULT 1,
    [ShowStaffID] [bit] NOT NULL DEFAULT 1,
    [ShowDepartment] [bit] NOT NULL DEFAULT 1,
    [ShowDOJ] [bit] NOT NULL DEFAULT 1,
    [ShowPhone] [bit] NOT NULL DEFAULT 1,
    [ShowBloodGroup] [bit] NOT NULL DEFAULT 1,
    [ShowStaffAddress] [bit] NOT NULL DEFAULT 1,
    [DesignType] [int] NOT NULL DEFAULT 1, -- 1: Horizontal, 2: Vertical
    [ShowBarcode] [bit] NOT NULL DEFAULT 1,
    
    -- System Fields
    [IsActive] [bit] NOT NULL DEFAULT 1,
    [IsDeleted] [bit] NOT NULL DEFAULT 0,
    [CreatedOn] [datetime] NOT NULL DEFAULT (getdate()),
    [CreatedBy] [int] NULL,
    [UpdatedOn] [datetime] NULL,
    [UpdatedBy] [int] NULL,
    [CompanyID] [int] NULL,
    [SessionID] [int] NULL,
PRIMARY KEY CLUSTERED ([IDCardID] ASC))
END
GO

-- SP: sp_StaffIDCard_GetAll
CREATE OR ALTER PROCEDURE [dbo].[sp_StaffIDCard_GetAll]
    @CompanyID INT,
    @SessionID INT
AS
BEGIN
    SELECT * FROM tbl_StaffIDCard 
    WHERE CompanyID = @CompanyID AND SessionID = @SessionID AND IsDeleted = 0
    ORDER BY IDCardID DESC
END
GO

-- SP: sp_StaffIDCard_GetByID
CREATE OR ALTER PROCEDURE [dbo].[sp_StaffIDCard_GetByID]
    @IDCardID INT
AS
BEGIN
    SELECT * FROM tbl_StaffIDCard WHERE IDCardID = @IDCardID AND IsDeleted = 0
END
GO

-- SP: sp_StaffIDCard_Upsert
CREATE OR ALTER PROCEDURE [dbo].[sp_StaffIDCard_Upsert]
    @IDCardID INT,
    @IDCardTitle NVARCHAR(200),
    @SchoolName NVARCHAR(200),
    @HeaderColor NVARCHAR(50),
    @AddressPhoneEmail NVARCHAR(MAX),
    @BackgroundImage VARBINARY(MAX) = NULL,
    @BackgroundImageType NVARCHAR(50) = NULL,
    @BackgroundImageName NVARCHAR(200) = NULL,
    @LogoImage VARBINARY(MAX) = NULL,
    @LogoImageType NVARCHAR(50) = NULL,
    @LogoImageName NVARCHAR(200) = NULL,
    @SignatureImage VARBINARY(MAX) = NULL,
    @SignatureImageType NVARCHAR(50) = NULL,
    @SignatureImageName NVARCHAR(200) = NULL,
    @ShowStaffName BIT,
    @ShowDesignation BIT,
    @ShowStaffID BIT,
    @ShowDepartment BIT,
    @ShowDOJ BIT,
    @ShowPhone BIT,
    @ShowBloodGroup BIT,
    @ShowStaffAddress BIT,
    @DesignType INT,
    @ShowBarcode BIT,
    @IsActive BIT,
    @UserId INT,
    @CompanyID INT,
    @SessionID INT
AS
BEGIN
    IF @IDCardID = 0
    BEGIN
        INSERT INTO tbl_StaffIDCard (
            IDCardTitle, SchoolName, HeaderColor, AddressPhoneEmail,
            BackgroundImage, BackgroundImageType, BackgroundImageName,
            LogoImage, LogoImageType, LogoImageName,
            SignatureImage, SignatureImageType, SignatureImageName,
            ShowStaffName, ShowDesignation, ShowStaffID, ShowDepartment, ShowDOJ,
            ShowPhone, ShowBloodGroup, ShowStaffAddress, DesignType, ShowBarcode,
            IsActive, CreatedBy, CompanyID, SessionID
        ) VALUES (
            @IDCardTitle, @SchoolName, @HeaderColor, @AddressPhoneEmail,
            @BackgroundImage, @BackgroundImageType, @BackgroundImageName,
            @LogoImage, @LogoImageType, @LogoImageName,
            @SignatureImage, @SignatureImageType, @SignatureImageName,
            @ShowStaffName, @ShowDesignation, @ShowStaffID, @ShowDepartment, @ShowDOJ,
            @ShowPhone, @ShowBloodGroup, @ShowStaffAddress, @DesignType, @ShowBarcode,
            @IsActive, @UserId, @CompanyID, @SessionID
        );
        SELECT 1 AS Result, 'Staff ID Card Template added successfully' AS Message;
    END
    ELSE
    BEGIN
        UPDATE tbl_StaffIDCard SET
            IDCardTitle = @IDCardTitle,
            SchoolName = @SchoolName,
            HeaderColor = @HeaderColor,
            AddressPhoneEmail = @AddressPhoneEmail,
            BackgroundImage = ISNULL(@BackgroundImage, BackgroundImage),
            BackgroundImageType = ISNULL(@BackgroundImageType, BackgroundImageType),
            BackgroundImageName = ISNULL(@BackgroundImageName, BackgroundImageName),
            LogoImage = ISNULL(@LogoImage, LogoImage),
            LogoImageType = ISNULL(@LogoImageType, LogoImageType),
            LogoImageName = ISNULL(@LogoImageName, LogoImageName),
            SignatureImage = ISNULL(@SignatureImage, SignatureImage),
            SignatureImageType = ISNULL(@SignatureImageType, SignatureImageType),
            SignatureImageName = ISNULL(@SignatureImageName, SignatureImageName),
            ShowStaffName = @ShowStaffName,
            ShowDesignation = @ShowDesignation,
            ShowStaffID = @ShowStaffID,
            ShowDepartment = @ShowDepartment,
            ShowDOJ = @ShowDOJ,
            ShowPhone = @ShowPhone,
            ShowBloodGroup = @ShowBloodGroup,
            ShowStaffAddress = @ShowStaffAddress,
            DesignType = @DesignType,
            ShowBarcode = @ShowBarcode,
            IsActive = @IsActive,
            UpdatedOn = GETDATE(),
            UpdatedBy = @UserId
        WHERE IDCardID = @IDCardID;
        SELECT 1 AS Result, 'Staff ID Card Template updated successfully' AS Message;
    END
END
GO

-- sp_StaffIDCard_Delete
CREATE OR ALTER PROCEDURE [dbo].[sp_StaffIDCard_Delete]
    @IDCardID INT,
    @UserId INT
AS
BEGIN
    UPDATE tbl_StaffIDCard SET IsDeleted = 1, UpdatedOn = GETDATE(), UpdatedBy = @UserId 
    WHERE IDCardID = @IDCardID;
    SELECT 1 AS Result, 'Staff ID Card Template deleted successfully' AS Message;
END
GO

-- sp_StaffIDCard_ToggleStatus
CREATE OR ALTER PROCEDURE [dbo].[sp_StaffIDCard_ToggleStatus]
    @IDCardID INT,
    @IsActive BIT,
    @UserId INT
AS
BEGIN
    UPDATE tbl_StaffIDCard SET IsActive = @IsActive, UpdatedOn = GETDATE(), UpdatedBy = @UserId 
    WHERE IDCardID = @IDCardID;
    SELECT 1 AS Result, 'Status updated successfully' AS Message;
END
GO
