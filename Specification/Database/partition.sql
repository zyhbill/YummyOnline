USE [YummyOnlineHotelBaseDB]

GO
CREATE PARTITION FUNCTION [DinePartitionFun](nvarchar(14)) AS RANGE RIGHT FOR VALUES ()


CREATE PARTITION SCHEME [DinePartitionSchema] AS PARTITION [DinePartitionFun] TO ([PRIMARY])


ALTER DATABASE [YummyOnlineHotelBaseDB] ADD FILEGROUP DinePartition1

ALTER DATABASE [YummyOnlineHotelBaseDB] ADD FILE(
	name = 'DinePartition1',
	filename = 'C:\Program Files\Microsoft SQL Server\MSSQL12.MSSQLSERVER\MSSQL\DATA\DinePartitions\DinePartition1.ndf',
	size=5mb,
	filegrowth=5mb
)TO FILEGROUP DinePartition1

GO
ALTER PARTITION SCHEME DinePartitionSchema NEXT USED DinePartition1
GO
alter partition function DinePartitionFun() split range(N'16041500000001')





--合并分区函数

alter partition function DinePartitionFun() 
merge range(N'16041400000001')

