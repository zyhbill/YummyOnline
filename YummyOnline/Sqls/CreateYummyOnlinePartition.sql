BEGIN TRANSACTION

CREATE PARTITION FUNCTION [DateTimePartitionFun](datetime) AS RANGE LEFT FOR VALUES ()

CREATE PARTITION SCHEME [DateTimePartitionScheme] AS PARTITION [DateTimePartitionFun] TO ([PRIMARY])

CREATE CLUSTERED INDEX [IX_DateTime]
  ON [dbo].[Logs] ( [DateTime] ASC )
  WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = ON, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [DateTimePartitionScheme]([DateTime])

COMMIT TRANSACTION
