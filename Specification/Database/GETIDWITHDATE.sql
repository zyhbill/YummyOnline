USE [YummyOnlineHotelBaseDB]
GO
ALTER FUNCTION [dbo].[getDineId](@n INT) RETURNS nvarchar(MAX) AS 
BEGIN 
	DECLARE @nowDate NVARCHAR(6), @oldId NVARCHAR(max), @newId int
	SELECT @nowDate = CONVERT(NVARCHAR(6), CURRENT_TIMESTAMP, 12)
	SELECT @oldId = ISNULL(MAX(Id),0) FROM Dines WITH(XLOCK,PAGLOCK)
	if left(@oldId,6) = @nowDate
		set @newId = 1 + RIGHT(@oldId, @n)
	else
		set @newId = 1
	RETURN @nowDate + RIGHT(REPLICATE('0',@n) + CAST(@newId AS NVARCHAR(MAX)),@n) 
END