
-- =====================
-- STORED PROCEDURES
-- =====================

IF OBJECT_ID('dbo.sp_GetUserById','P') IS NOT NULL DROP PROCEDURE dbo.sp_GetUserById;
GO
CREATE PROCEDURE dbo.sp_GetUserById @id INT AS
BEGIN SELECT * FROM Users WHERE UserId=@id; END;
GO

IF OBJECT_ID('dbo.sp_GetOrderTotal','P') IS NOT NULL DROP PROCEDURE dbo.sp_GetOrderTotal;
GO
CREATE PROCEDURE dbo.sp_GetOrderTotal @orderId INT, @total DECIMAL(10,2) OUTPUT AS
BEGIN SELECT @total = Total FROM Orders WHERE OrderId=@orderId; END;
GO

-- Multi result sets
IF OBJECT_ID('dbo.sp_MultiResultTest','P') IS NOT NULL DROP PROCEDURE dbo.sp_MultiResultTest;
GO
CREATE PROCEDURE dbo.sp_MultiResultTest AS
BEGIN
    SELECT TOP 2 * FROM Users;
    SELECT COUNT(*) AS TotalOrders FROM Orders;
END;
GO

-- Insert returning output id
IF OBJECT_ID('dbo.sp_InsertOrderWithOutputId','P') IS NOT NULL DROP PROCEDURE dbo.sp_InsertOrderWithOutputId;
GO
CREATE PROCEDURE dbo.sp_InsertOrderWithOutputId 
@userId INT, @total DECIMAL(10,2), @newId INT OUTPUT AS
BEGIN 
    INSERT INTO Orders(UserId,Total) VALUES(@userId,@total);
    SET @newId = SCOPE_IDENTITY();
END;
GO

-- Table-valued parameter PROC
IF TYPE_ID('dbo.OrderTableType') IS NOT NULL DROP TYPE dbo.OrderTableType;
GO
CREATE TYPE dbo.OrderTableType AS TABLE(UserId INT, Total DECIMAL(10,2));
GO

IF OBJECT_ID('dbo.sp_BulkInsertOrders','P') IS NOT NULL DROP PROCEDURE dbo.sp_BulkInsertOrders;
GO
CREATE PROCEDURE dbo.sp_BulkInsertOrders @orders dbo.OrderTableType READONLY AS
BEGIN INSERT INTO Orders(UserId,Total) SELECT UserId,Total FROM @orders; END;
GO
