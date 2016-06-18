using HotelDAO.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Threading.Tasks;

namespace HotelDAO {
	public partial class HotelManager : BaseHotelManager {
		public HotelManager(string connString) : base(connString) { }

		public async Task RecordLog(Log.LogLevel level, string message, string detail = null) {
			Log log = new Log {
				Level = level,
				Message = message,
				Detail = detail
			};
			ctx.Logs.Add(log);
			await ctx.SaveChangesAsync();
		}
		public async Task<dynamic> GetLogs(DateTime date, int? count) {
			IQueryable<Log> linq = ctx.Logs.Where(p => SqlFunctions.DateDiff("day", p.DateTime, date) == 0)
				.OrderByDescending(p => p.Id);
			if(count != null) {
				linq = linq.Take((int)count);
			}

			return await linq.Select(p => new {
				Level = p.Level.ToString(),
				p.Message,
				p.Detail,
				p.DateTime
			}).ToListAsync();
		}
	}

	public partial class HotelManager {
		public async Task<HotelConfig> GetHotelConfig() {
			return await ctx.HotelConfigs.FirstOrDefaultAsync();
		}

		public async Task<dynamic> GetAreas() {
			return await ctx.Areas.Where(p => p.Usable == true).Select(p => new {
				p.Id,
				p.Name,
				p.Description,
				p.DepartmentReciptId,
				p.DepartmentServeId,
			}).ToListAsync();
		}

		public async Task<dynamic> GetDesks() {
			return await ctx.Desks.Where(p => p.Usable)
				.Select(p => new {
					Area = new {
						p.Area.Id,
						p.Area.Name
					},
					p.Id,
					p.QrCode,
					p.Name,
					p.Description,
					p.Status,
					p.Order,
					p.HeadCount,
					p.MinPrice,
				})
				.ToListAsync();
		}
		public async Task<Desk> GetDeskByQrCode(string qrCode) {
			return await ctx.Desks.FirstOrDefaultAsync(p => p.QrCode == qrCode);
		}

		public async Task<dynamic> GetMenuClasses() {
			var linq = ctx.MenuClasses
				.Where(p => p.IsShow && p.Usable)
				.Select(p => new {
					p.Id,
					p.Name,
					p.Menus.Count
				});
			return await linq.ToListAsync();
		}
		public async Task<dynamic> GetMenus(MenuStatus status = MenuStatus.Normal) {
			var linq = ctx.Menus
				.Where(p => p.Usable && p.Status == status)
				.Select(p => new {
					p.Id,
					p.Code,
					p.Name,
					p.NameAbbr,
					p.PicturePath,
					p.IsFixed,
					p.SupplyDate,
					p.Unit,
					p.MinOrderCount,
					p.Ordered,
					Remarks = p.Remarks.Select(pp => new {
						pp.Id,
						pp.Name,
						pp.Price
					}),
					MenuClasses = p.Classes.Where(pp => pp.IsShow).Select(pp => pp.Id),
					p.MenuPrice,
				});
			var menuList = await linq.ToListAsync();
			menuList.RemoveAll(p => { // 将当天不供应的菜品过滤
				int date = p.SupplyDate >> 6 - ((int)DateTime.Now.DayOfWeek);
				return date % 2 == 0;
			});
			return menuList;
		}
		public async Task<dynamic> GetMenuOnSales() {
			DayOfWeek week = DateTime.Now.DayOfWeek;
			var linq = ctx.MenuOnSales
				.Where(p => p.OnSaleWeek == week)
				.Select(p => new {
					p.Id,
					p.Price,
				});
			return await linq.ToListAsync();
		}
		public async Task<dynamic> GetMenuSetMeals() {
			var linq = ctx.MenuSetMeals
				.Select(p => new {
					p.MenuSetId,
					p.Count,
					Menu = new {
						p.Menu.Id,
						p.Menu.Name,
						p.Menu.MenuPrice.Price,
						p.Menu.MenuPrice.Discount,
						p.Menu.Ordered,
					}
				});

			return await linq.ToListAsync();
		}
		public async Task<bool> ToggleMenuStatus(string menuId, MenuStatus status) {
			var menu = await ctx.Menus.Where(m => m.Id == menuId).FirstOrDefaultAsync();
			if(menu == null) {
				return false;
			}
			if(!Enum.IsDefined(status.GetType(), status)) {
				return false;
			}
			menu.Status = status;
			ctx.SaveChanges();
			return true;
		}

		public async Task<dynamic> GetRemarks() {
			return await ctx.Remarks
				.Select(p => new {
					p.Id,
					p.Name,
					p.Price
				})
				.ToListAsync();
		}

		public async Task<PayKind> GetPayKindById(int payKindId) {
			return await ctx.PayKinds.FirstOrDefaultAsync(p => p.Id == payKindId);
		}
		public async Task<dynamic> GetPayKinds(List<PayKindType> payKindTypes) {
			var linq = formatPayKind(ctx.PayKinds
				.Where(p => p.Usable && payKindTypes.Contains(p.Type)));
			return await linq.ToListAsync();
		}
		public async Task<int> GetOtherPayKindId() {
			return await ctx.PayKinds
				.Where(p => p.Type == PayKindType.Other)
				.Select(p => p.Id)
				.FirstOrDefaultAsync();
		}
		public async Task<dynamic> GetOtherPayKind() {
			return await formatPayKind(ctx.PayKinds
				.Where(p => p.Type == PayKindType.Other))
				.FirstOrDefaultAsync();
		}
		private IQueryable<dynamic> formatPayKind(IQueryable<PayKind> query) {
			return query.Select(p => new {
				p.Id,
				p.Name,
				p.Type,
				p.Description,
				p.Discount
			});
		}

		public async Task<List<TimeDiscount>> GetTimeDiscounts() {
			DayOfWeek week = DateTime.Now.DayOfWeek;
			var linq = ctx.TimeDiscounts.Where(p => p.Week == week);
			return await linq.ToListAsync();
		}
		public async Task<List<VipDiscount>> GetVipDiscounts() {
			var linq = ctx.VipDiscounts;
			return await linq.ToListAsync();
		}
	}
}
