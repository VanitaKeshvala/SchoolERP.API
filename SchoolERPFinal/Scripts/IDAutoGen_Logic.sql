CREATE OR ALTER PROCEDURE sp_Settings_IDAutoGen_GetNext
    @EntityType NVARCHAR(50),
    @CompanyID INT,
    @SessionID INT,
    @FieldValues NVARCHAR(MAX) = NULL -- JSON string containing field values
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Prefix NVARCHAR(50), @DigitCount INT, @StartNo INT, @IsEnabled BIT;
    
    -- Get Configuration (Try current session, fallback to most recent for company)
    SELECT TOP 1 @Prefix = LTRIM(RTRIM(Prefix)), @DigitCount = DigitCount, @StartNo = StartNo, @IsEnabled = ISNULL(IsEnabled, 0)
    FROM tbl_Settings_IDAutoGen
    WHERE UPPER(EntityType) = UPPER(@EntityType) AND (CompanyId = @CompanyID OR @CompanyID = 0)
    ORDER BY (CASE WHEN SessionId = @SessionID THEN 0 ELSE 1 END), ConfigID DESC;
    
    -- If no config found, or disabled, return default logic
    IF @IsEnabled = 1
    BEGIN
        DECLARE @CurrentCount INT = 0;
        
        -- Get current record count for the entity
        IF @EntityType = 'Staff'
            SELECT @CurrentCount = COUNT(*) FROM tbl_HR_Staff WHERE CompanyID = @CompanyID;
        ELSE IF @EntityType = 'Student'
            SELECT @CurrentCount = COUNT(*) FROM tbl_StudentAdmission WHERE CompanyID = @CompanyID;
        
        DECLARE @NextNo INT = ISNULL(@StartNo, 1) + @CurrentCount;
        DECLARE @FormattedNo NVARCHAR(MAX) = CAST(@NextNo AS NVARCHAR(MAX));
        
        -- Pad with zeros
        SET @DigitCount = ISNULL(@DigitCount, 4);
        WHILE LEN(@FormattedNo) < @DigitCount
        BEGIN
            SET @FormattedNo = '0' + @FormattedNo;
        END

        -- Replace dynamic placeholders in Prefix (e.g., {Class}, {Section})
        IF @FieldValues IS NOT NULL AND ISJSON(@FieldValues) > 0
        BEGIN
            -- Replace common placeholders. For a fully dynamic approach, a recursive CTE or cursor would be needed.
            DECLARE @ClassVal NVARCHAR(100) = JSON_VALUE(@FieldValues, '$.Class');
            IF @ClassVal IS NOT NULL SET @Prefix = REPLACE(@Prefix, '{Class}', @ClassVal);

            DECLARE @SectionVal NVARCHAR(100) = JSON_VALUE(@FieldValues, '$.Section');
            IF @SectionVal IS NOT NULL SET @Prefix = REPLACE(@Prefix, '{Section}', @SectionVal);
            
            DECLARE @YearVal NVARCHAR(100) = JSON_VALUE(@FieldValues, '$.Year');
            IF @YearVal IS NOT NULL SET @Prefix = REPLACE(@Prefix, '{Year}', @YearVal);
        END
        
        SELECT ISNULL(@Prefix, '') + @FormattedNo AS NextID;
    END
    ELSE
    BEGIN
        -- Fallback: If disabled or not configured, return empty to allow backend default
        SELECT '' AS NextID;
    END
END
GO
