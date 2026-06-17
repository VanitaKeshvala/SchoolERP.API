-- =============================================
-- ALTER TABLES to include CompanyID and SessionID
-- =============================================

-- Update tbl_Mst_Fields
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbl_Mst_Fields') AND name = 'CompanyId')
BEGIN
    ALTER TABLE tbl_Mst_Fields ADD CompanyId INT;
END
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbl_Mst_Fields') AND name = 'SessionId')
BEGIN
    ALTER TABLE tbl_Mst_Fields ADD SessionId INT;
END
GO

-- Update tbl_Settings_IDAutoGen
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbl_Settings_IDAutoGen') AND name = 'CompanyId')
BEGIN
    ALTER TABLE tbl_Settings_IDAutoGen ADD CompanyId INT;
END
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbl_Settings_IDAutoGen') AND name = 'SessionId')
BEGIN
    ALTER TABLE tbl_Settings_IDAutoGen ADD SessionId INT;
END
GO

-- =============================================
-- UPDATE STORED PROCEDURES
-- =============================================

-- sp_Mst_Fields_GetAll
CREATE OR ALTER PROCEDURE sp_Mst_Fields_GetAll
    @CompanyId INT,
    @SessionId INT,
    @IsSystemField BIT = NULL,
    @BelongsTo NVARCHAR(50) = NULL
AS
BEGIN
    SELECT * FROM tbl_Mst_Fields 
    WHERE (CompanyId = @CompanyId OR IsSystemField = 1) -- System fields are global, custom fields are per company
    AND (SessionId = @SessionId OR IsSystemField = 1)
    AND (@IsSystemField IS NULL OR IsSystemField = @IsSystemField)
    AND (@BelongsTo IS NULL OR BelongsTo = @BelongsTo)
    ORDER BY DisplayOrder;
END
GO

-- sp_Mst_Fields_Upsert
CREATE OR ALTER PROCEDURE sp_Mst_Fields_Upsert
    @FieldId INT,
    @BelongsTo NVARCHAR(50),
    @FieldName NVARCHAR(100),
    @FieldType NVARCHAR(50),
    @FieldValues NVARCHAR(MAX),
    @IsSystemField BIT,
    @IsRequired BIT,
    @IsActive BIT,
    @DisplayOrder INT,
    @GridColumn INT,
    @OnTable BIT,
    @CompanyId INT,
    @SessionId INT,
    @UserId INT
AS
BEGIN
    IF @FieldId > 0
    BEGIN
        UPDATE tbl_Mst_Fields SET
            BelongsTo = @BelongsTo,
            FieldName = @FieldName,
            FieldType = @FieldType,
            FieldValues = @FieldValues,
            IsSystemField = @IsSystemField,
            IsRequired = @IsRequired,
            IsActive = @IsActive,
            DisplayOrder = @DisplayOrder,
            GridColumn = @GridColumn,
            OnTable = @OnTable,
            CompanyId = @CompanyId,
            SessionId = @SessionId,
            ModifiedOn = GETDATE(),
            ModifiedBy = @UserId
        WHERE FieldId = @FieldId;
        SELECT 1 AS Result, 'Field updated successfully' AS Message;
    END
    ELSE
    BEGIN
        INSERT INTO tbl_Mst_Fields (BelongsTo, FieldName, FieldType, FieldValues, IsSystemField, IsRequired, IsActive, DisplayOrder, GridColumn, OnTable, CompanyId, SessionId, CreatedOn, CreatedBy)
        VALUES (@BelongsTo, @FieldName, @FieldType, @FieldValues, @IsSystemField, @IsRequired, @IsActive, @DisplayOrder, @GridColumn, @OnTable, @CompanyId, @SessionId, GETDATE(), @UserId);
        SELECT 1 AS Result, 'Field added successfully' AS Message;
    END
END
GO

-- sp_Settings_IDAutoGen_Get
CREATE OR ALTER PROCEDURE sp_Settings_IDAutoGen_Get
    @CompanyId INT,
    @SessionId INT
AS
BEGIN
    SELECT * FROM tbl_Settings_IDAutoGen 
    WHERE CompanyId = @CompanyId AND SessionId = @SessionId;
END
GO

-- sp_Settings_IDAutoGen_Upsert
CREATE OR ALTER PROCEDURE sp_Settings_IDAutoGen_Upsert
    @EntityType NVARCHAR(50),
    @IsEnabled BIT,
    @Prefix NVARCHAR(50),
    @DigitCount INT,
    @StartNo INT,
    @FieldsToInclude NVARCHAR(MAX),
    @CompanyId INT,
    @SessionId INT,
    @UserId INT
AS
BEGIN
    IF EXISTS (SELECT 1 FROM tbl_Settings_IDAutoGen WHERE EntityType = @EntityType AND CompanyId = @CompanyId AND SessionId = @SessionId)
    BEGIN
        UPDATE tbl_Settings_IDAutoGen
        SET IsEnabled = @IsEnabled,
            Prefix = @Prefix,
            DigitCount = @DigitCount,
            StartNo = @StartNo,
            FieldsToInclude = @FieldsToInclude,
            UpdatedAt = GETDATE()
        WHERE EntityType = @EntityType AND CompanyId = @CompanyId AND SessionId = @SessionId;
    END
    ELSE
    BEGIN
        INSERT INTO tbl_Settings_IDAutoGen (EntityType, IsEnabled, Prefix, DigitCount, StartNo, FieldsToInclude, CompanyId, SessionId)
        VALUES (@EntityType, @IsEnabled, @Prefix, @DigitCount, @StartNo, @FieldsToInclude, @CompanyId, @SessionId);
    END
END
GO
