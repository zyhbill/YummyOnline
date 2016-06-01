ALTER DATABASE [@@databaseName] ADD FILE(
	name = '@@fileGroupName',
	filename = '@@path\@@fileGroupName.ndf' 
) TO FILEGROUP @@fileGroupName