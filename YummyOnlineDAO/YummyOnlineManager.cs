using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Threading.Tasks;
using YummyOnlineDAO.Models;

namespace YummyOnlineDAO {
	public class YummyOnlineManager {
		public YummyOnlineManager() {
			ctx = new YummyOnlineContext();
		}
		protected YummyOnlineContext ctx;

		public string ConnectionString {
			get {
				return ctx.Database.Connection.ConnectionString;
			}
		}

		public async Task<SystemConfig> GetSystemConfig() {
			return await ctx.SystemConfigs.FirstOrDefaultAsync();
		}

		public async Task RecordLog(Log.LogProgram program, Log.LogLevel level, string message, string detail = null) {
			Log log = new Log {
				Program = program,
				Level = level,
				Message = message,
				Detail = detail
			};
			ctx.Logs.Add(log);
			await ctx.SaveChangesAsync();
		}

		public async Task<List<NewDineInformClientGuid>> GetGuids() {
			return await ctx.NewDineInformClientGuids.OrderBy(p => p.Description).ToListAsync();
		}

		public async Task<List<Hotel>> GetHotels() {
			return await ctx.Hotels.Where(p => p.ConnectionString != null).ToListAsync();
		}
		public async Task<string> GetHotelConnectionStringById(int id) {
			return await ctx.Hotels.Where(p => p.Id == id).Select(p => p.ConnectionString).FirstOrDefaultAsync();
		}
		public async Task<Hotel> GetHotelById(int id) {
			return await ctx.Hotels.FirstOrDefaultAsync(p => p.Id == id);
		}
	}
}
