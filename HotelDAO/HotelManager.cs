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

		public async Task<Customer> GetOrCreateCustomerOrCreate(string userId) {
			Customer customer = await ctx.Customers.FirstOrDefaultAsync(p => p.Id == userId);
			if(customer == null) {
				customer = new Customer {
					Id = userId,
					Points = 0,
				};
				ctx.Customers.Add(customer);
				await ctx.SaveChangesAsync();
			}
			return customer;
		}

		public bool GetNeedCodeImgSync() {
			return ctx.HotelConfigs.FirstOrDefault().NeedCodeImg;
		}

		public async Task<HotelConfig> GetHotelConfig() {
			var t = ctx.HotelConfigs.FirstOrDefaultAsync();
			return await t.ConfigureAwait(false);
		}

		public async Task<PayKind> GetPayKind(int payKindId) {
			return await ctx.PayKinds.FirstOrDefaultAsync(p => p.Id == payKindId);
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
		public async Task<dynamic> GetMenus() {
			var linq = ctx.Menus
				.Where(p => p.Usable && p.Status == MenuStatus.Normal)
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

		public async Task<dynamic> GetPayKinds() {
			var linq = ctx.PayKinds
				.Where(p => p.Usable && p.Type == PayKindType.Online || p.Type == PayKindType.Other)
				.Select(p => new {
					p.Id,
					p.Name,
					p.Description,
					p.Discount,
					p.Type,
					p.RedirectUrl
				});
			return await linq.ToListAsync();
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

		public async Task<DinePaidDetail> GetDineOnlinePaidDetail(string dineId) {
			Dine dine = await ctx.Dines
				.Include(p => p.DinePaidDetails.Select(pp => pp.PayKind))
				.FirstOrDefaultAsync(p => p.Id == dineId);
			if(dine != null && dine.IsOnline) {
				foreach(DinePaidDetail paidDetail in dine.DinePaidDetails) {
					if(paidDetail.PayKind.Type == PayKindType.Online) {
						return paidDetail;
					}
				}
			}
			return null;
		}

		public async Task<List<dynamic>> GetHistoryDines(string userId) {
			return await FormatDines(ctx.Dines
				.Where(p => p.UserId == userId)
				.OrderByDescending(p => p.Id))
				.ToListAsync();
		}
		public async Task<int> GetHistoryDinesCount(string userId) {
			return await ctx.Dines.CountAsync(p => p.UserId == userId);
		}


		public async Task<bool> TransferOrders(string oldUserId, string newUserId) {
			try {
				List<Dine> dines = await ctx.Dines.Where(p => p.UserId == oldUserId).ToListAsync();
				foreach(Dine d in dines) {
					d.UserId = newUserId;
				}
				await ctx.SaveChangesAsync();
				return true;
			}
			catch(Exception e) {
				await RecordLog(Log.LogLevel.Error, e.Message);
				return false;
			}
		}

		public async Task RecordLog(Log.LogLevel level, string message) {
			Log log = new Log {
				Level = level,
				Message = message
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
				p.DateTime
			}).ToListAsync();
		}

		public static IQueryable<dynamic> FormatDines(IQueryable<Dine> dine) {
			return dine.Select(p => new {
				p.Id,
				p.Status,
				p.Type,
				p.HeadCount,
				p.Price,
				p.OriPrice,
				p.Discount,
				p.DiscountName,
				p.BeginTime,
				p.IsPaid,
				p.IsOnline,
				p.ClerkId,
				p.WaiterId,
				p.UserId,
				Remarks = p.Remarks.Select(pp => new {
					pp.Id,
					pp.Name
				}),
				Desk = new {
					p.Desk.Id,
					p.Desk.QrCode,
					p.Desk.Name,
					p.Desk.Description
				},
				DineMenus = p.DineMenus.Select(d => new {
					d.Status,
					d.Count,
					d.OriPrice,
					d.Price,
					d.RemarkPrice,
					Remarks = d.Remarks.Select(r => new {
						r.Id,
						r.Name
					}),
					Menu = new {
						d.Menu.Id,
						d.Menu.Code,
						d.Menu.Name,
						d.Menu.NameAbbr,
						d.Menu.PicturePath,
						d.Menu.Unit
					}
				}),
				DinePaidDetails = p.DinePaidDetails.Select(d => new {
					d.Price,
					d.RecordId,
					PayKind = new {
						d.PayKind.Id,
						d.PayKind.Name,
						d.PayKind.Type
					}
				})
			});
		}
	}
}
