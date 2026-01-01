
-- =====================
-- FUNCTION SCRIPT
-- =====================

-- IF OBJECT_ID('dbo.fn_AddTwoInts','FN') IS NOT NULL DROP FUNCTION dbo.fn_AddTwoInts;
-- GO
CREATE FUNCTION dbo.fn_AddTwoInts(@a INT, @b INT)
RETURNS INT AS BEGIN RETURN @a + @b; END;
GO

-- IF OBJECT_ID('dbo.fn_ConcatStrings','FN') IS NOT NULL DROP FUNCTION dbo.fn_ConcatStrings;
-- GO
CREATE FUNCTION dbo.fn_ConcatStrings(@a NVARCHAR(50), @b NVARCHAR(50))
RETURNS NVARCHAR(101) AS BEGIN RETURN @a + '-' + @b; END;
GO

-- Table-valued
-- IF OBJECT_ID('dbo.fn_GetUserOrders','IF') IS NOT NULL DROP FUNCTION dbo.fn_GetUserOrders;
-- GO
CREATE FUNCTION dbo.fn_GetUserOrders(@userId INT)
RETURNS TABLE AS RETURN (
    SELECT OrderId, Total, OrderDate FROM Orders WHERE UserId=@userId
);
GO

-- IF OBJECT_ID('dbo.fn_GetProductsByPriceRange','IF') IS NOT NULL DROP FUNCTION dbo.fn_GetProductsByPriceRange;
-- GO
CREATE FUNCTION dbo.fn_GetProductsByPriceRange(@min DECIMAL(10,2), @max DECIMAL(10,2))
RETURNS TABLE AS RETURN (
    SELECT * FROM Products WHERE Price BETWEEN @min AND @max
);
GO
