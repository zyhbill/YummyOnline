using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YummyOnlineDAO.Models {
	public class SystemConfig {
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		/// <summary>
		/// tcp服务IP
		/// </summary>
		[MaxLength(15), Required]
		public string TcpServerIp { get; set; }
		/// <summary>
		/// tcp服务端口
		/// </summary>
		public int TcpServerPort { get; set; }
		/// <summary>
		/// tcp服务器目录
		/// </summary>
		[Required]
		public string TcpServerDir { get; set; }
		[Required]
		public string OrderSystemUrl { get; set; }
		[Required]
		public string ManagementUrl { get; set; }
		public int WebSocketPort { get; set; }
		public int ManagementWebSocketPort { get; set; }
		[Required]
		public string Token { get; set; }

		[Required]
		public string DefaultConnectionString { get; set; }
		[Required]
		public string DefaultAdminConnectionString { get; set; }
	}

	public class NewDineInformClientGuid {
		[Key]
		public Guid Guid { get; set; }
		public string Description { get; set; }
	}
}
