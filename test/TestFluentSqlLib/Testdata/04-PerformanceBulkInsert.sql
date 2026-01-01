
-- =====================
-- BULK INSERT TEST
-- =====================

DECLARE @i INT=1;
WHILE @i <= 2000
BEGIN 
    INSERT INTO TestAllTypes(BitCol,IntCol,MoneyCol,FloatCol,RealCol,GuidCol,XmlCol,JsonCol)
    VALUES (1,@i,10.5,1.1,2.2,NEWID(),'<v/>','{"id":'+CAST(@i AS NVARCHAR)+'}');
    SET @i+=1;
END
GO
