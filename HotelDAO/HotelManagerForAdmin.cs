using HotelDAO.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Threading.Tasks;

namespace HotelDAO {
	public partial class HotelManagerForAdmin : BaseHotelManager {
		public HotelManagerForAdmin(string connStr)
			: base(connStr) { }

		public async Task<int> GetDineCount() {
			return await ctx.Dines.CountAsync();
		}
		public async Task<int> GetDineCount(DateTime dateTime) {
			return await ctx.Dines.CountAsync(p => SqlFunctions.DateDiff("d", p.BeginTime, dateTime) == 0);
		}
	}
}
