-- =============================================
-- Table: tbl_Settings_IDAutoGen
-- Description: Stores configuration for auto-generating IDs for different entities.
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tbl_Settings_IDAutoGen')
BEGIN
    CREATE TABLE tbl_Settings_IDAutoGen (
        ConfigID INT PRIMARY KEY IDENTITY(1,1),
        EntityType NVARCHAR(50) NOT NULL, -- 'Student', 'Staff'
        IsEnabled BIT DEFAULT 0,
        Prefix NVARCHAR(50),
        DigitCount INT DEFAULT 4,
        StartNo INT DEFAULT 1,
        FieldsToInclude NVARCHAR(MAX), -- Comma-separated FieldNames
        CreatedAt DATETIME DEFAULT GETDATE(),
        UpdatedAt DATETIME
    );
END
GO

-- =============================================
-- Procedure: sp_Settings_IDAutoGen_Get
-- =============================================
CREATE OR ALTER PROCEDURE sp_Settings_IDAutoGen_Get
AS
BEGIN
    SELECT * FROM tbl_Settings_IDAutoGen;
END
GO

-- =============================================
-- Procedure: sp_Settings_IDAutoGen_Upsert
-- =============================================
CREATE OR ALTER PROCEDURE sp_Settings_IDAutoGen_Upsert
    @EntityType NVARCHAR(50),
    @IsEnabled BIT,
    @Prefix NVARCHAR(50),
    @DigitCount INT,
    @StartNo INT,
    @FieldsToInclude NVARCHAR(MAX),
    @UserId INT
AS
BEGIN
    IF EXISTS (SELECT 1 FROM tbl_Settings_IDAutoGen WHERE EntityType = @EntityType)
    BEGIN
        UPDATE tbl_Settings_IDAutoGen
        SET IsEnabled = @IsEnabled,
            Prefix = @Prefix,
            DigitCount = @DigitCount,
            StartNo = @StartNo,
            FieldsToInclude = @FieldsToInclude,
            UpdatedAt = GETDATE()
        WHERE EntityType = @EntityType;
    END
    ELSE
    BEGIN
        INSERT INTO tbl_Settings_IDAutoGen (EntityType, IsEnabled, Prefix, DigitCount, StartNo, FieldsToInclude)
        VALUES (@EntityType, @IsEnabled, @Prefix, @DigitCount, @StartNo, @FieldsToInclude);
    END
END
GO
