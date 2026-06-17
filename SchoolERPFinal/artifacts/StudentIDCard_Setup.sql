-- Table: tbl_StudentIDCard
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tbl_StudentIDCard]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[tbl_StudentIDCard](
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
    [ShowAdmissionNo] [bit] NOT NULL DEFAULT 1,
    [ShowStudentName] [bit] NOT NULL DEFAULT 1,
    [ShowClass] [bit] NOT NULL DEFAULT 1,
    [ShowFatherName] [bit] NOT NULL DEFAULT 1,
    [ShowMotherName] [bit] NOT NULL DEFAULT 1,
    [ShowStudentAddress] [bit] NOT NULL DEFAULT 1,
    [ShowPhone] [bit] NOT NULL DEFAULT 1,
    [ShowDOB] [bit] NOT NULL DEFAULT 1,
    [ShowBloodGroup] [bit] NOT NULL DEFAULT 1,
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
ELSE
BEGIN
    -- If table exists, ensure DesignType is INT if it was BIT before
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[tbl_StudentIDCard]') AND name = 'ShowDesignType')
    BEGIN
        EXEC sp_rename 'tbl_StudentIDCard.ShowDesignType', 'DesignType', 'COLUMN';
        ALTER TABLE tbl_StudentIDCard ALTER COLUMN DesignType INT NOT NULL;
    END
END
GO

-- SP: sp_StudentIDCard_GetAll
CREATE OR ALTER PROCEDURE [dbo].[sp_StudentIDCard_GetAll]
    @CompanyID INT,
    @SessionID INT
AS
BEGIN
    SELECT * FROM tbl_StudentIDCard 
    WHERE CompanyID = @CompanyID AND SessionID = @SessionID AND IsDeleted = 0
    ORDER BY IDCardID DESC
END
GO

-- SP: sp_StudentIDCard_GetByID
CREATE OR ALTER PROCEDURE [dbo].[sp_StudentIDCard_GetByID]
    @IDCardID INT
AS
BEGIN
    SELECT * FROM tbl_StudentIDCard WHERE IDCardID = @IDCardID AND IsDeleted = 0
END
GO

-- SP: sp_StudentIDCard_Upsert
CREATE OR ALTER PROCEDURE [dbo].[sp_StudentIDCard_Upsert]
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
    @ShowAdmissionNo BIT,
    @ShowStudentName BIT,
    @ShowClass BIT,
    @ShowFatherName BIT,
    @ShowMotherName BIT,
    @ShowStudentAddress BIT,
    @ShowPhone BIT,
    @ShowDOB BIT,
    @ShowBloodGroup BIT,
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
        INSERT INTO tbl_StudentIDCard (
            IDCardTitle, SchoolName, HeaderColor, AddressPhoneEmail,
            BackgroundImage, BackgroundImageType, BackgroundImageName,
            LogoImage, LogoImageType, LogoImageName,
            SignatureImage, SignatureImageType, SignatureImageName,
            ShowAdmissionNo, ShowStudentName, ShowClass, ShowFatherName, ShowMotherName,
            ShowStudentAddress, ShowPhone, ShowDOB, ShowBloodGroup, DesignType, ShowBarcode,
            IsActive, CreatedBy, CompanyID, SessionID
        ) VALUES (
            @IDCardTitle, @SchoolName, @HeaderColor, @AddressPhoneEmail,
            @BackgroundImage, @BackgroundImageType, @BackgroundImageName,
            @LogoImage, @LogoImageType, @LogoImageName,
            @SignatureImage, @SignatureImageType, @SignatureImageName,
            @ShowAdmissionNo, @ShowStudentName, @ShowClass, @ShowFatherName, @ShowMotherName,
            @ShowStudentAddress, @ShowPhone, @ShowDOB, @ShowBloodGroup, @DesignType, @ShowBarcode,
            @IsActive, @UserId, @CompanyID, @SessionID
        );
        SELECT 1 AS Result, 'ID Card Template added successfully' AS Message;
    END
    ELSE
    BEGIN
        UPDATE tbl_StudentIDCard SET
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
            ShowAdmissionNo = @ShowAdmissionNo,
            ShowStudentName = @ShowStudentName,
            ShowClass = @ShowClass,
            ShowFatherName = @ShowFatherName,
            ShowMotherName = @ShowMotherName,
            ShowStudentAddress = @ShowStudentAddress,
            ShowPhone = @ShowPhone,
            ShowDOB = @ShowDOB,
            ShowBloodGroup = @ShowBloodGroup,
            DesignType = @DesignType,
            ShowBarcode = @ShowBarcode,
            IsActive = @IsActive,
            UpdatedOn = GETDATE(),
            UpdatedBy = @UserId
        WHERE IDCardID = @IDCardID;
        SELECT 1 AS Result, 'ID Card Template updated successfully' AS Message;
    END
END
GO
