ALTER DATABASE [@@dataBaseName] ADD FILE(
	name = '@@fileGroupName',
	filename = '@@path\@@fileGroupName.ndf' 
) TO FILEGROUP @@fileGroupName