SELECT fg.data_space_id AS DataSpaceId,
       fg.name AS FileGroupName,
       f.name AS [FileName],
       f.physical_name AS FilePath,
       f.size AS SIZE,
       f.growth AS Growth
FROM sys.filegroups AS fg
JOIN sys.database_files AS f ON f.data_space_id = fg.data_space_id