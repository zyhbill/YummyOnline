using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YummyOnlineDAO.Models {
	public class Log {
		public enum LogLevel {
			Debug = 0,
			Error = 1,
			Info = 2,
			Success = 3,
			Warning = 4
		}
		public enum LogProgram {
			/// <summary>
			/// 点菜系统
			/// </summary>
			OrderSystem = 0,
			/// <summary>
			/// 身份验证系统
			/// </summary>
			Identity = 1,
			TcpServer = 2,
			/// <summary>
			/// YummyOnline管理系统
			/// </summary>
			System = 3,
			/// <summary>
			/// 收银饭店管理系统
			/// </summary>
			Manager = 4,
			/// <summary>
			/// 其他远程日志
			/// </summary>
			Remote = 5,
			OrderSystem_Waiter = 6
		}
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public DateTime DateTime { get; set; }
		public LogProgram Program { get; set; }
		public LogLevel Level { get; set; }
		public string Message { get; set; }
		public string Detail { get; set; }
	}
}
