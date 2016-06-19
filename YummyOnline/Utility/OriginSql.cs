using Protocal;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace YummyOnline.Utility {
	public class DbPartitionInfo {
		public string TableName { get; set; }
		public string IndexName { get; set; }
		public int IndexId { get; set; }
		public string PartitionScheme { get; set; }
		public int PartitionNumber { get; set; }
		public string FileGroupName { get; set; }
		public DateTime? LowerBoundaryValue { get; set; }
		public DateTime? UpperBoundaryValue { get; set; }
		public string Range { get; set; }
		public long Rows { get; set; }
	}
	public class FileGroupInfo {
		public int DataSpaceId { get; set; }
		public string FileGroupName { get; set; }
		public string FileName { get; set; }
		public string FilePath { get; set; }
		public int Size { get; set; }
		public int Growth { get; set; }
	}

	public class OriginSql {
		private DbContext ctx;
		public string sqlsPath;

		public OriginSql(string connStr) {
			ctx = new DbContext(connStr);
			DirectoryInfo bin = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
			sqlsPath = $"{bin.FullName}\\Sqls";
		}
		public async Task<List<DbPartitionInfo>> GetDbPartitionInfos() {
			string sql = File.ReadAllText($"{sqlsPath}\\GetDbPartitionInfos.sql");
			return await ctx.Database.SqlQuery<DbPartitionInfo>(sql).ToListAsync();
		}
		public async Task<List<FileGroupInfo>> GetFileGroupInfos() {
			string sql = File.ReadAllText($"{sqlsPath}\\GetFileGroupInfos.sql");
			return await ctx.Database.SqlQuery<FileGroupInfo>(sql).ToListAsync();
		}
		public async Task<FunctionResult> CreateYummyOnlinePartition() {
			string sql = File.ReadAllText($"{sqlsPath}\\CreateYummyOnlinePartition.sql");
			return await executeSql(sql);
		}
		public async Task<FunctionResult> CreateHotelPartition() {
			string sql = File.ReadAllText($"{sqlsPath}\\CreateHotelPartition.sql");
			return await executeSql(sql);
		}

		public async Task<FunctionResult> Split(DateTime dateTime) {
			dateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day);
			List<DbPartitionInfo> partitions = await GetDbPartitionInfos();
			if(partitions.Count == 0) {
				return new FunctionResult(false, "数据库未分区，无法分割");
			}

			// 获得默认数据库路径
			string sql = "SELECT serverproperty('InstanceDefaultDataPath') AS DefaultDataPath";
			string defaultPath = await ctx.Database.SqlQuery<string>(sql).FirstOrDefaultAsync();

			string dataBaseName = ctx.Database.Connection.Database;

			int next = partitions.LastOrDefault().PartitionNumber;
			string fileGroupName = $"Partition{next}";

			string path = $"{defaultPath}\\{dataBaseName}";

			FunctionResult result = await AddFileGroup(dataBaseName, fileGroupName);
			if(!result.Succeeded) {
				return new FunctionResult(false, $"AddFileGroup Failed, {result.Message}");
			}

			result = await AddFile(dataBaseName, path, fileGroupName);
			if(!result.Succeeded) {
				return new FunctionResult(false, $"AddFile Failed, {result.Message}");
			}

			result = await AlterScheme(fileGroupName);
			if(!result.Succeeded) {
				return new FunctionResult(false, $"AlterScheme Failed, {result.Message}");
			}

			result = await SplitFunction(dateTime);
			if(!result.Succeeded) {
				return new FunctionResult(false, $"SplitFunction Failed, {result.Message}");
			}

			return new FunctionResult();
		}

		/// <summary>
		/// 增加文件组
		/// </summary>
		public async Task<FunctionResult> AddFileGroup(string dataBaseName, string fileGroupName) {
			var fileGroups = await GetFileGroupInfos();
			if(fileGroups.Exists(p => p.FileGroupName == fileGroupName)) {
				return new FunctionResult();
			}

			string sql = $"ALTER DATABASE [{dataBaseName}] ADD FILEGROUP [{fileGroupName}]";

			return await executeSql(sql);
		}
		/// <summary>
		/// 增加文件
		/// </summary>
		public async Task<FunctionResult> AddFile(string dataBaseName, string path, string fileGroupName) {
			if(!Directory.Exists(path)) {
				Directory.CreateDirectory(path);
			}

			var fileGroups = await GetFileGroupInfos();
			if(fileGroups.Exists(p => p.FileName == fileGroupName)) {
				return new FunctionResult();
			}

			string sql = $@"ALTER DATABASE [{dataBaseName}] ADD FILE(
							 name = '{fileGroupName}',
							 filename = '{path}\{fileGroupName}.ndf' 
						 ) TO FILEGROUP {fileGroupName}";

			return await executeSql(sql);
		}
		public async Task<FunctionResult> AlterScheme(string fileGroupName) {
			string sql = $"ALTER PARTITION SCHEME DateTimePartitionScheme NEXT USED [{fileGroupName}]";

			return await executeSql(sql);
		}
		public async Task<FunctionResult> SplitFunction(DateTime splitCondition) {
			string sql = $"ALTER PARTITION FUNCTION DateTimePartitionFun() split range('{splitCondition.ToString("yyyy-MM-dd")}')";

			return await executeSql(sql);
		}

		public async Task<FunctionResult> Merge(DateTime dateTime) {
			List<DbPartitionInfo> partitions = await GetDbPartitionInfos();
			if(partitions.Count == 0) {
				return new FunctionResult(false, "数据库未分区，无法合并");
			}

			string sql = $"ALTER PARTITION FUNCTION DateTimePartitionFun() merge range('{dateTime.ToString("yyyy-MM-dd")}')";

			return await executeSql(sql);
		}

		public async Task<FunctionResult> CreateDatabase(string databaseName) {
			return await executeSql($"CREATE DATABASE [{databaseName}]");
		}
		public async Task<FunctionResult> InitializeHotel() {
			string sql = File.ReadAllText($"{sqlsPath}\\InitializeDatabase.sql");
			return await executeSql(sql);
		}

		public async Task<FunctionResult> Backup(string path) {
			string databaseName = ctx.Database.Connection.Database;
			string sql = $"BACKUP DATABASE [{databaseName}] TO DISK = N'{path}\\{databaseName}.bak'";
			return await executeSql(sql);
		}

		public async Task<FunctionResult> ExecuteSql(string sql) {
			return await executeSql(sql);
		}

		private async Task<FunctionResult> executeSql(string sql) {
			string[] sqls = sql.Split(new string[] { "GO", "Go", "go" }, StringSplitOptions.RemoveEmptyEntries);
			foreach(string s in sqls) {
				try {
					await ctx.Database.ExecuteSqlCommandAsync(TransactionalBehavior.DoNotEnsureTransaction, s);
				}
				catch(Exception e) {
					return new FunctionResult(false, e.Message);
				}
			}
			return new FunctionResult();
		}
	}
}