using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Web.Administration;

namespace YummyOnline.Utility {
	public class SiteInfo {
		public long Id { get; set; }
		public string Name { get; set; }
		public string PhysicalPath { get; set; }
		public string ApplicationPoolName { get; set; }
		public string Protocal { get; set; }
		public bool AutoStart { get; set; }
		public string State { get; set; }
		public string Address { get; set; }
		public int Port { get; set; }
		public string Host { get; set; }
	}
	public class W3wpInfo {
		public int Pid { get; set; }
		public string ApplicationPoolName { get; set; }
		public string State { get; set; }
	}

	public static class IISManager {
		private static ServerManager sm {
			get {
				return new ServerManager();
			}
		}

		public static List<SiteInfo> GetSites() {
			List<SiteInfo> infos = new List<SiteInfo>();
			
			foreach(Site s in sm.Sites) {
				string protocal = s.Bindings[0].Protocol;
				SiteInfo info = new SiteInfo {
					Id = s.Id,
					Name = s.Name,
					PhysicalPath = s.Applications[0].VirtualDirectories["/"].PhysicalPath,
					ApplicationPoolName = s.Applications[0].ApplicationPoolName,
					Protocal = s.Bindings[0].Protocol
				};
				if(protocal == "http") {
					info.State = s.State.ToString();
					info.Address = s.Bindings[0].EndPoint.Address.ToString();
					info.Port = s.Bindings[0].EndPoint.Port;
					info.Host = s.Bindings[0].Host;
				}
				infos.Add(info);
			}
			return infos;
		}

		public static bool StartSite(long id) {
			Site site = getSiteById(id);
			if(site == null || site.State == ObjectState.Started || site.State == ObjectState.Starting) {
				return false;
			}
			site.Start();
			return true;
		}
		public static bool StopSite(long id) {
			Site site = getSiteById(id);
			if(site == null || site.State == ObjectState.Stopped || site.State == ObjectState.Stopping) {
				return false;
			}
			site.Stop();
			return true;
		}

		private static Site getSiteById(long id) {
			foreach(Site s in sm.Sites) {
				if(s.Id == id) {
					return s;
				}
			}
			return null;
		}

		public static List<W3wpInfo> GetWorkerProcesses() {
			List<W3wpInfo> w3wps = new List<W3wpInfo>();
			foreach(WorkerProcess w3wp in sm.WorkerProcesses) {
				w3wps.Add(new W3wpInfo {
					Pid = w3wp.ProcessId,
					ApplicationPoolName = w3wp.AppPoolName,
					State = w3wp.State.ToString()
				});
			}
			return w3wps;
		}
	}
}