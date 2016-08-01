using HotelDAO.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace HotelDAO {
	public partial class BaseHotelManager {
		public async Task<dynamic> GetFormatedMenuClasses() {
			var linq = ctx.MenuClasses
				.Where(p => p.IsShow && p.Usable)
				.Select(p => new {
					p.Id,
					p.Name,
					p.Menus.Count
				});
			return await linq.ToListAsync();
		}
		public async Task<dynamic> GetFormatedMenus(MenuStatus status = MenuStatus.Normal) {
			var linq = ctx.Menus
				.Where(p => p.Usable && !p.IsOnlyInSetMeal && p.Status == status)
				.Select(p => new {
					p.Id,
					p.Code,
					p.Name,
					p.EnglishName,
					p.NameAbbr,
					p.PicturePath,
					p.IsFixed,
					p.IsSetMeal,
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
		public async Task<dynamic> GetFormatedMenuOnSales() {
			DayOfWeek week = DateTime.Now.DayOfWeek;
			var linq = ctx.MenuOnSales
				.Where(p => p.OnSaleWeek == week && p.Menu.Usable && p.Menu.Status == MenuStatus.Normal)
				.Select(p => new {
					p.Id,
					p.Price,
				});
			return await linq.ToListAsync();
		}
		public async Task<dynamic> GetFormatedMenuSetMeals() {
			var linq = ctx.SetMealClasses.GroupBy(p => p.SetMealId)
				.Select(p => new {
					SetMealId = p.Key,
					Classes = p.Select(c => new {
						c.Name,
						c.Count,
						Menus = c.SetMealClassMenus.Select(m => new {
							m.Count,
							Menu = new {
								m.Menu.Id,
								m.Menu.Code,
								m.Menu.Name,
								m.Menu.EnglishName,
								m.Menu.NameAbbr,
								m.Menu.PicturePath,
								m.Menu.Unit,
								m.Menu.MinOrderCount,
								m.Menu.Ordered,
								m.Menu.MenuPrice,
							}
						})
					})
				});

			return await linq.ToListAsync();
		}

		public async Task<dynamic> GetFormatedPayKinds(List<PayKindType> payKindTypes) {
			var linq = formatPayKind(ctx.PayKinds
				.Where(p => p.Usable && payKindTypes.Contains(p.Type)));
			return await linq.ToListAsync();
		}
		protected IQueryable<dynamic> formatPayKind(IQueryable<PayKind> query) {
			return query.Select(p => new {
				p.Id,
				p.Name,
				p.Type,
				p.Description,
				p.Discount
			});
		}
		public async Task<int> GetOtherPayKindId() {
			return await ctx.PayKinds
				.Where(p => p.Type == PayKindType.Other)
				.Select(p => p.Id)
				.FirstOrDefaultAsync();
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