using HotelDAO.Models;
using System.Threading.Tasks;

namespace HotelDAO {
	public abstract class BaseHotelManager {
		public BaseHotelManager(string connString) {
			ctx = new HotelContext(connString);
		}
		protected HotelContext ctx;

		public async Task RecordLog(Log.LogLevel level, string message, string detail = null) {
			Log log = new Log {
				Level = level,
				Message = message,
				Detail = detail
			};
			ctx.Logs.Add(log);
			await ctx.SaveChangesAsync();
		}
	}
}
