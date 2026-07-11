
-- =====================
-- STORED PROCEDURES
-- =====================

-- IF OBJECT_ID('dbo.sp_GetUserById','P') IS NOT NULL DROP PROCEDURE dbo.sp_GetUserById;
-- GO
CREATE PROCEDURE dbo.sp_GetUserById @id INT AS
BEGIN SELECT * FROM Users WHERE UserId=@id; END;
GO

-- IF OBJECT_ID('dbo.sp_GetOrderTotal','P') IS NOT NULL DROP PROCEDURE dbo.sp_GetOrderTotal;
-- GO
CREATE PROCEDURE dbo.sp_GetOrderTotal @orderId INT, @total DECIMAL(10,2) OUTPUT AS
BEGIN SELECT @total = Total FROM Orders WHERE OrderId=@orderId; END;
GO

-- Multi result sets
-- IF OBJECT_ID('dbo.sp_MultiResultTest','P') IS NOT NULL DROP PROCEDURE dbo.sp_MultiResultTest;
-- GO
CREATE PROCEDURE dbo.sp_MultiResultTest AS
BEGIN
    SELECT TOP 2 * FROM Users;
    SELECT COUNT(*) AS TotalOrders FROM Orders;
END;
GO

-- Insert returning output id
-- IF OBJECT_ID('dbo.sp_InsertOrderWithOutputId','P') IS NOT NULL DROP PROCEDURE dbo.sp_InsertOrderWithOutputId;
-- GO
CREATE PROCEDURE dbo.sp_InsertOrderWithOutputId 
@userId INT, @total DECIMAL(10,2), @newId INT OUTPUT AS
BEGIN 
    INSERT INTO Orders(UserId,Total) VALUES(@userId,@total);
    SET @newId = SCOPE_IDENTITY();
END;
GO

-- Table-valued parameter PROC
-- IF TYPE_ID('dbo.OrderTableType') IS NOT NULL DROP TYPE dbo.OrderTableType;
-- GO
CREATE TYPE dbo.OrderTableType AS TABLE(UserId INT, Total DECIMAL(10,2));
GO

-- IF OBJECT_ID('dbo.sp_BulkInsertOrders','P') IS NOT NULL DROP PROCEDURE dbo.sp_BulkInsertOrders;
-- GO
CREATE PROCEDURE dbo.sp_BulkInsertOrders @orders dbo.OrderTableType READONLY AS
BEGIN INSERT INTO Orders(UserId,Total) SELECT UserId,Total FROM @orders; END;
GO

-- Wide table-valued parameter type for multi-type / nullable coverage
CREATE TYPE dbo.WideTableType AS TABLE(
    IntCol INT,
    BitCol BIT,
    DecimalCol DECIMAL(18,4),
    FloatCol FLOAT,
    GuidCol UNIQUEIDENTIFIER,
    DateCol DATETIME2,
    TextCol NVARCHAR(200) NULL,
    NullableIntCol INT NULL
);
GO

-- Echoes a wide TVP straight back as a result set, for TVP round-trip testing
CREATE PROCEDURE dbo.sp_EchoWideTvp @rows dbo.WideTableType READONLY AS
BEGIN
    SELECT IntCol, BitCol, DecimalCol, FloatCol, GuidCol, DateCol, TextCol, NullableIntCol
    FROM @rows
    ORDER BY IntCol;
END;
GO
