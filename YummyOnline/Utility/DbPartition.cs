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

	public class DbPartition {
		private DbContext ctx;
		public string sqlsPath;

		public DbPartition(string connStr) {
			ctx = new DbContext(connStr);
			DirectoryInfo bin = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
			sqlsPath = $"{bin.FullName}\\Sqls";
		}
		public async Task<List<DbPartitionInfo>> GetDbPartitionInfos() {
			string sql = File.ReadAllText($"{sqlsPath}\\GetDbPartitionInfos.sql");
			return await ctx.Database.SqlQuery<DbPartitionInfo>(sql).ToListAsync();
		}
		public async Task<FunctionResult> CreatePartitionFunAndSchema() {
			string sql = File.ReadAllText($"{sqlsPath}\\CreatePartitionFunAndSchema.sql");
			return await executeSql(sql);
		}

		public async Task<FunctionResult> Split(DateTime dateTime) {
			dateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day);
			List<DbPartitionInfo> partitions = await GetDbPartitionInfos();
			if(partitions.Count == 0) {
				return new FunctionResult(false, "数据库未分区，无法分割");
			}

			string sql = File.ReadAllText($"{sqlsPath}\\GetDefaultDataPath.sql");
			string defaultPath = await ctx.Database.SqlQuery<string>(sql).FirstOrDefaultAsync();

			string dataBaseName = ctx.Database.Connection.Database;

			int next = partitions.LastOrDefault().PartitionNumber;
			string fileGroupName = $"Partition{next}";

			string path = $"{defaultPath}\\{dataBaseName}";

			FunctionResult result = await AddFileGroup(dataBaseName, fileGroupName);
			if(!result.Succeeded) {
				return result;
			}

			result = await AddFile(dataBaseName, path, fileGroupName);
			if(!result.Succeeded) {
				return result;
			}

			result = await AlterScheme(fileGroupName);
			if(!result.Succeeded) {
				return result;
			}

			result = await SplitFunction(dateTime);
			if(!result.Succeeded) {
				return result;
			}

			return new FunctionResult();
		}

		public async Task<FunctionResult> AddFileGroup(string dataBaseName, string fileGroupName) {
			string sql = File.ReadAllText($"{sqlsPath}\\AddFileGroup.sql");
			sql = sql.Replace("@@dataBaseName", dataBaseName);
			sql = sql.Replace("@@fileGroupName", fileGroupName);

			return await executeSql(sql);
		}
		public async Task<FunctionResult> AddFile(string dataBaseName, string path, string fileGroupName) {
			if(!Directory.Exists(path)) {
				Directory.CreateDirectory(path);
			}

			string sql = File.ReadAllText($"{sqlsPath}\\AddFile.sql");
			sql = sql.Replace("@@dataBaseName", dataBaseName);
			sql = sql.Replace("@@fileGroupName", fileGroupName);
			sql = sql.Replace("@@path", path);

			return await executeSql(sql);
		}
		public async Task<FunctionResult> AlterScheme(string fileGroupName) {
			string sql = File.ReadAllText($"{sqlsPath}\\AlterScheme.sql");
			sql = sql.Replace("@@fileGroupName", fileGroupName);

			return await executeSql(sql);
		}
		public async Task<FunctionResult> SplitFunction(DateTime splitCondition) {
			splitCondition = new DateTime(splitCondition.Year, splitCondition.Month, splitCondition.Day);
			string sql = File.ReadAllText($"{sqlsPath}\\SplitFunction.sql");
			sql = sql.Replace("@@splitCondition", splitCondition.ToString("yyyy-MM-dd"));

			return await executeSql(sql);
		}

		public async Task<FunctionResult> Merge(DateTime dateTime) {
			List<DbPartitionInfo> partitions = await GetDbPartitionInfos();
			if(partitions.Count == 0) {
				return new FunctionResult(false, "数据库未分区，无法合并");
			}

			string sql = File.ReadAllText($"{sqlsPath}\\Merge.sql");
			sql = sql.Replace("@@mergeCondition", dateTime.ToString("yyyy-MM-dd"));

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