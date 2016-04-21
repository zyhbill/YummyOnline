using HotelDAO.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace HotelDAO {
	public class HotelManagerForTcpServer : BaseHotelManager {
		public HotelManagerForTcpServer(string connStr)
			: base(connStr) { }
		public async Task<Dine> GetDineById(string dineId) {
			Dine dine = await ctx.Dines
				.Include(p => p.DineMenus.Select(pp => pp.Menu).Select(pp => pp.Department).Select(pp => pp.Printer))
				.Include(p => p.DineMenus.Select(pp => pp.Remarks))
				.Include(p => p.Desk.Area.DepartmentRecipt.Printer)
				.Include(p => p.Desk.Area.DepartmentServe.Printer)
				.Include(p => p.DinePaidDetails.Select(pp => pp.PayKind))
				.FirstOrDefaultAsync(p => p.Id == dineId);
			return dine;
		}
		public async Task<List<MenuSetMeal>> GetMenuSetMealByMenuSetId(string menuSetId) {
			List<MenuSetMeal> menuSetMeals = await ctx.MenuSetMeals.Include(p => p.Menu).ToListAsync();
			return menuSetMeals;
		}
	}
}