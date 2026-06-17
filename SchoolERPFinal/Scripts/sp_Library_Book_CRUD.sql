USE [SchoolERP]
GO

-- ======================================================
-- Procedure: sp_Library_Book_CRUD
-- Description: Handles Library Book operations
-- ======================================================
CREATE OR ALTER PROCEDURE sp_Library_Book_CRUD
    @Action NVARCHAR(20),
    @BookID INT = NULL,
    @BookTitle NVARCHAR(200) = NULL,
    @BookNo NVARCHAR(50) = NULL,
    @ISBNNo NVARCHAR(50) = NULL,
    @Publisher NVARCHAR(200) = NULL,
    @Author NVARCHAR(200) = NULL,
    @Subject NVARCHAR(200) = NULL,
    @RackNo NVARCHAR(50) = NULL,
    @TotalQty INT = NULL,
    @BookPrice DECIMAL(18,2) = NULL,
    @PostDate DATETIME = NULL,
    @Description NVARCHAR(1000) = NULL,
    @CompanyID INT = NULL,
    @UserID INT = NULL,
    @SearchTerm NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Action = 'LIST'
    BEGIN
        SELECT 
            BookID, BookTitle, BookNo, ISBNNo, Publisher, Author, Subject, 
            RackNo, TotalQty, AvailableQty, BookPrice, PostDate, Description, IsActive
        FROM tbl_Library_Books
        WHERE CompanyID = @CompanyID AND IsDelete = 0
          AND (@SearchTerm IS NULL OR BookTitle LIKE '%' + @SearchTerm + '%' OR Author LIKE '%' + @SearchTerm + '%' OR BookNo LIKE '%' + @SearchTerm + '%')
        ORDER BY CreatedOn DESC;
    END

    ELSE IF @Action = 'GETBYID'
    BEGIN
        SELECT * FROM tbl_Library_Books WHERE BookID = @BookID;
    END

    ELSE IF @Action = 'SAVE'
    BEGIN
        IF @BookID IS NULL OR @BookID = 0
        BEGIN
            INSERT INTO tbl_Library_Books (BookTitle, BookNo, ISBNNo, Publisher, Author, Subject, RackNo, TotalQty, AvailableQty, BookPrice, PostDate, Description, CompanyID, CreatedBy, CreatedOn)
            VALUES (@BookTitle, @BookNo, @ISBNNo, @Publisher, @Author, @Subject, @RackNo, @TotalQty, @TotalQty, @BookPrice, @PostDate, @Description, @CompanyID, @UserID, GETDATE());
            SELECT 1 AS Result, 'Book added successfully' AS Message;
        END
        ELSE
        BEGIN
            -- Logic for AvailableQty: If TotalQty increases, increase AvailableQty too
            DECLARE @OldTotal INT;
            SELECT @OldTotal = TotalQty FROM tbl_Library_Books WHERE BookID = @BookID;

            UPDATE tbl_Library_Books
            SET BookTitle = @BookTitle, 
                BookNo = @BookNo, 
                ISBNNo = @ISBNNo, 
                Publisher = @Publisher, 
                Author = @Author, 
                Subject = @Subject, 
                RackNo = @RackNo, 
                TotalQty = @TotalQty, 
                AvailableQty = AvailableQty + (@TotalQty - @OldTotal), -- Adjust available qty based on total change
                BookPrice = @BookPrice, 
                PostDate = @PostDate, 
                Description = @Description, 
                ModifiedBy = @UserID, 
                ModifiedOn = GETDATE()
            WHERE BookID = @BookID;
            SELECT 1 AS Result, 'Book details updated successfully' AS Message;
        END
    END

    ELSE IF @Action = 'DELETE'
    BEGIN
        UPDATE tbl_Library_Books SET IsDelete = 1, ModifiedBy = @UserID, ModifiedOn = GETDATE() WHERE BookID = @BookID;
        SELECT 1 AS Result, 'Book deleted successfully' AS Message;
    END

    ELSE IF @Action = 'TOGGLESTATUS'
    BEGIN
        UPDATE tbl_Library_Books SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END, ModifiedBy = @UserID, ModifiedOn = GETDATE() WHERE BookID = @BookID;
        SELECT 1 AS Result, 'Status updated successfully' AS Message;
    END
END
GO
