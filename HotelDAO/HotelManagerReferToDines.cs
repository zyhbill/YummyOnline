using HotelDAO.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelDAO {
	public partial class HotelManager {
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

		public async Task<dynamic> GetDineById(string dineId) {
			dynamic dine = await formatDine(ctx.Dines
				.Where(p => p.Id == dineId))
				.FirstOrDefaultAsync();
			return dine;
		}
		public async Task<dynamic> GetDineForPrintingById(string dineId) {
			dynamic dine = await formatDineForPrinting(ctx.Dines
					.Where(p => p.Id == dineId))
					.FirstOrDefaultAsync();
			return dine;
		}
		public async Task<dynamic> GetMenuSetMealByMenuSetId(string menuSetId) {
			dynamic menuSetMeals = await ctx.MenuSetMeals
				.Select(p => new {
					p.Menu.Id,
					p.Menu.Name,
					p.Count
				}).ToListAsync();
			return menuSetMeals;
		}

		public async Task<List<dynamic>> GetHistoryDines(string userId) {
			return await formatDine(ctx.Dines
				.Where(p => p.UserId == userId)
				.OrderByDescending(p => p.Id))
				.ToListAsync();
		}
		public async Task<int> GetHistoryDinesCount(string userId) {
			return await ctx.Dines.CountAsync(p => p.UserId == userId);
		}

		public async Task<List<dynamic>> GetWaiterCurrentDines(string waiterId, string deskId) {
			List<dynamic> dines = await formatDine(ctx.Dines
				.Where(p => p.Desk.Id == deskId && p.Status != DineStatus.Shifted)
				.OrderByDescending(p => p.Id))
				.ToListAsync();
			return dines;
		}

		public async Task<bool> IsDinePaid(string dineId) {
			bool isPaid = await ctx.Dines.Where(p => p.Id == dineId).Select(p => p.IsPaid).FirstOrDefaultAsync();
			return isPaid;
		}

		public async Task ShiftDines() {
			var dines = ctx.Dines.Where(d => d.IsPaid == true && d.Status != DineStatus.Shifted);
			foreach(var dine in dines) {
				dine.Status = DineStatus.Shifted;
			}
			await ctx.SaveChangesAsync();
		}

		public async Task<int> GetDineCount(DateTime dateTime) {
			return await ctx.Dines.CountAsync(p => SqlFunctions.DateDiff("day", p.BeginTime, dateTime) == 0);
		}
		public async Task<int[]> GetDinePerHourCount(DateTime dateTime) {
			int[] counts = new int[24];
			for(int i = 0; i < 24; i++) {
				DateTime from = dateTime.Date.AddHours(i);
				DateTime to = dateTime.Date.AddHours(i + 1);
				counts[i] = await ctx.Dines.CountAsync(p => p.BeginTime >= from && p.BeginTime < to);
			}
			return counts;
		}
		public async Task<int> GetDineCount(string userId) {
			return await ctx.Dines.Where(p => p.UserId == userId).CountAsync();
		}

		public async Task<bool> TransferDines(string oldUserId, string newUserId) {
			try {
				List<Dine> dines = await ctx.Dines.Where(p => p.UserId == oldUserId).ToListAsync();
				foreach(Dine d in dines) {
					d.UserId = newUserId;
				}
				await ctx.SaveChangesAsync();
				return true;
			}
			catch {
				return false;
			}
		}

		public IQueryable<dynamic> formatDine(IQueryable<Dine> dine) {
			return dine.Select(p => new {
				p.Id,
				p.Status,
				p.Type,
				p.HeadCount,
				p.Price,
				p.OriPrice,
				p.Discount,
				p.DiscountName,
				p.DiscountType,
				p.BeginTime,
				p.IsPaid,
				p.IsOnline,
				p.UserId,
				Waiter = new {
					p.Waiter.Id,
					p.Waiter.Name
				},
				Clerk = new {
					p.Clerk.Id,
					p.Clerk.Name
				},
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

		public IQueryable<dynamic> formatDineForPrinting(IQueryable<Dine> dine) {
			return dine.Select(p => new {
				p.Id,
				p.Status,
				p.Type,
				p.HeadCount,
				p.Price,
				p.OriPrice,
				p.Discount,
				p.DiscountName,
				p.DiscountType,
				p.BeginTime,
				p.IsPaid,
				p.IsOnline,
				p.UserId,
				Waiter = new {
					p.Waiter.Id,
					p.Waiter.Name
				},
				Clerk = new {
					p.Clerk.Id,
					p.Clerk.Name
				},
				Remarks = p.Remarks.Select(pp => new {
					pp.Id,
					pp.Name,
					pp.Price
				}),
				Desk = new {
					p.Desk.Id,
					p.Desk.QrCode,
					p.Desk.Name,
					p.Desk.Description,
					ReciptPrinter = new {
						p.Desk.Area.DepartmentRecipt.Printer.Id,
						p.Desk.Area.DepartmentRecipt.Printer.Name,
						p.Desk.Area.DepartmentRecipt.Printer.IpAddress,
						p.Desk.Area.DepartmentRecipt.Printer.Usable
					},
					ServePrinter = new {
						p.Desk.Area.DepartmentServe.Printer.Id,
						p.Desk.Area.DepartmentServe.Printer.Name,
						p.Desk.Area.DepartmentServe.Printer.IpAddress,
						p.Desk.Area.DepartmentServe.Printer.Usable
					}
				},
				DineMenus = p.DineMenus.Select(d => new {
					d.Status,
					d.Count,
					d.OriPrice,
					d.Price,
					d.RemarkPrice,
					Remarks = d.Remarks.Select(r => new {
						r.Id,
						r.Name,
						r.Price
					}),
					Menu = new {
						d.Menu.Id,
						d.Menu.Code,
						d.Menu.Name,
						d.Menu.NameAbbr,
						d.Menu.Unit,
						d.Menu.IsSetMeal,
						Printer = new {
							d.Menu.Department.Printer.Id,
							d.Menu.Department.Printer.Name,
							d.Menu.Department.Printer.IpAddress,
							d.Menu.Department.Printer.Usable
						}
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