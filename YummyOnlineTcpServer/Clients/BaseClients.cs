using AsynchronousTcp;
using Newtonsoft.Json;
using Protocol;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using YummyOnlineDAO;
using YummyOnlineDAO.Models;

namespace YummyOnlineTcpServer {
	public abstract class BaseClients {
		public BaseClients(Action<string, Log.LogLevel> log, Action<TcpClient, object> send) {
			this.log = log;
			this.send = send;
		}

		protected Action<string, Log.LogLevel> log;
		protected Action<TcpClient, object> send;

		public abstract void HandleTimeOut();
		public abstract void HandleError(TcpClient client, Exception e);
	}
}
