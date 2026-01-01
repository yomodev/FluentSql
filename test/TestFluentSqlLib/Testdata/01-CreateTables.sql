
-- =====================
-- TABLE CREATION SCRIPT
-- =====================

-- IF OBJECT_ID('dbo.Orders', 'U') IS NOT NULL DROP TABLE dbo.Orders;
-- IF OBJECT_ID('dbo.Products', 'U') IS NOT NULL DROP TABLE dbo.Products;
-- IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
-- IF OBJECT_ID('dbo.TestAllTypes', 'U') IS NOT NULL DROP TABLE dbo.TestAllTypes;

CREATE TABLE dbo.Users (
    UserId INT IDENTITY PRIMARY KEY,
    FirstName NVARCHAR(50),
    LastName NVARCHAR(50),
    Email NVARCHAR(150) UNIQUE,
    CreatedAt DATETIME2 DEFAULT SYSDATETIME()
);
GO

CREATE TABLE dbo.Products (
    ProductId INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(100),
    Price DECIMAL(10,2),
    Stock INT
);
GO

CREATE TABLE dbo.Orders (
    OrderId INT IDENTITY PRIMARY KEY,
    UserId INT FOREIGN KEY REFERENCES Users(UserId),
    Total DECIMAL(10,2),
    OrderDate DATETIME2 DEFAULT SYSDATETIME()
);
GO

-- Wide table for type testing
CREATE TABLE dbo.TestAllTypes (
    Id INT IDENTITY PRIMARY KEY,
    BitCol BIT,
    IntCol INT,
    MoneyCol MONEY,
    FloatCol FLOAT,
    RealCol REAL,
    GuidCol UNIQUEIDENTIFIER,
    XmlCol XML,
    JsonCol NVARCHAR(MAX),
    Created DATETIME2 DEFAULT SYSUTCDATETIME()
);
GO
