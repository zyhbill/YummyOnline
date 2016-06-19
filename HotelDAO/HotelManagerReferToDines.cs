using HotelDAO.Models;
using Protocal;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
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
		public async Task<DineForPrintingProtocal.DineForPrinting> GetDineForPrintingById(string dineId) {
			var dine = await formatDineForPrinting(ctx.Dines
					.Where(p => p.Id == dineId))
					.FirstOrDefaultAsync();

			foreach(var dineMenu in dine.DineMenus) {
				if(dineMenu.Menu.IsSetMeal) {
					dineMenu.Menu.SetMealMenus = await ctx.MenuSetMeals
					.Where(p => p.MenuSetId == dineMenu.Menu.Id)
					.Select(p => new DineForPrintingProtocal.SetMealMenu {
						Id = p.Menu.Id,
						Name = p.Menu.Name,
						Count = p.Count
					}).ToListAsync();
				}
			}
			return dine;
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

		public IQueryable<DineForPrintingProtocal.DineForPrinting> formatDineForPrinting(IQueryable<Dine> dine) {
			return dine.Select(p => new DineForPrintingProtocal.DineForPrinting {
				Id = p.Id,
				Status = p.Status,
				Type = p.Type,
				HeadCount = p.HeadCount,
				Price = p.Price,
				OriPrice = p.OriPrice,
				Discount = p.Discount,
				DiscountName = p.DiscountName,
				BeginTime = p.BeginTime,
				IsPaid = p.IsPaid,
				IsOnline = p.IsOnline,
				UserId = p.UserId,
				Waiter = new DineForPrintingProtocal.Staff {
					Id = p.Waiter.Id,
					Name = p.Waiter.Name
				},
				Clerk = new DineForPrintingProtocal.Staff {
					Id = p.Clerk.Id,
					Name = p.Clerk.Name
				},
				Remarks = p.Remarks.Select(pp => new DineForPrintingProtocal.Remark {
					Id = pp.Id,
					Name = pp.Name,
					Price = pp.Price
				}).ToList(),
				Desk = new DineForPrintingProtocal.Desk {
					Id = p.Desk.Id,
					QrCode = p.Desk.QrCode,
					Name = p.Desk.Name,
					Description = p.Desk.Description,
					ReciptPrinter = new DineForPrintingProtocal.Printer {
						Id = p.Desk.Area.DepartmentRecipt.Printer.Id,
						Name = p.Desk.Area.DepartmentRecipt.Printer.Name,
						IpAddress = p.Desk.Area.DepartmentRecipt.Printer.IpAddress,
						Usable = p.Desk.Area.DepartmentRecipt.Printer.Usable
					},
					ServePrinter = new DineForPrintingProtocal.Printer {
						Id = p.Desk.Area.DepartmentServe.Printer.Id,
						Name = p.Desk.Area.DepartmentServe.Printer.Name,
						IpAddress = p.Desk.Area.DepartmentServe.Printer.IpAddress,
						Usable = p.Desk.Area.DepartmentServe.Printer.Usable
					}
				},
				DineMenus = p.DineMenus.Select(d => new DineForPrintingProtocal.DineMenu {
					Status = d.Status,
					Count = d.Count,
					OriPrice = d.OriPrice,
					Price = d.Price,
					RemarkPrice = d.RemarkPrice,
					Remarks = d.Remarks.Select(r => new DineForPrintingProtocal.Remark {
						Id = r.Id,
						Name = r.Name,
						Price = r.Price
					}).ToList(),
					Menu = new DineForPrintingProtocal.Menu {
						Id = d.Menu.Id,
						Code = d.Menu.Code,
						Name = d.Menu.Name,
						NameAbbr = d.Menu.NameAbbr,
						Unit = d.Menu.Unit,
						IsSetMeal = d.Menu.IsSetMeal,
						Printer = new DineForPrintingProtocal.Printer {
							Id = d.Menu.Department.Printer.Id,
							Name = d.Menu.Department.Printer.Name,
							IpAddress = d.Menu.Department.Printer.IpAddress,
							Usable = d.Menu.Department.Printer.Usable
						}
					}
				}).ToList()
			});


			//return dine.Select(p => new {
			//	p.Id,
			//	p.Status,
			//	p.Type,
			//	p.HeadCount,
			//	p.Price,
			//	p.OriPrice,
			//	p.Discount,
			//	p.DiscountName,
			//	p.DiscountType,
			//	p.BeginTime,
			//	p.IsPaid,
			//	p.IsOnline,
			//	p.UserId,
			//	Waiter = new {
			//		p.Waiter.Id,
			//		p.Waiter.Name
			//	},
			//	Clerk = new {
			//		p.Clerk.Id,
			//		p.Clerk.Name
			//	},
			//	Remarks = p.Remarks.Select(pp => new {
			//		pp.Id,
			//		pp.Name,
			//		pp.Price
			//	}),
			//	Desk = new {
			//		p.Desk.Id,
			//		p.Desk.QrCode,
			//		p.Desk.Name,
			//		p.Desk.Description,
			//		ReciptPrinter = new {
			//			p.Desk.Area.DepartmentRecipt.Printer.Id,
			//			p.Desk.Area.DepartmentRecipt.Printer.Name,
			//			p.Desk.Area.DepartmentRecipt.Printer.IpAddress,
			//			p.Desk.Area.DepartmentRecipt.Printer.Usable
			//		},
			//		ServePrinter = new {
			//			p.Desk.Area.DepartmentServe.Printer.Id,
			//			p.Desk.Area.DepartmentServe.Printer.Name,
			//			p.Desk.Area.DepartmentServe.Printer.IpAddress,
			//			p.Desk.Area.DepartmentServe.Printer.Usable
			//		}
			//	},
			//	DineMenus = p.DineMenus.Select(d => new {
			//		d.Status,
			//		d.Count,
			//		d.OriPrice,
			//		d.Price,
			//		d.RemarkPrice,
			//		Remarks = d.Remarks.Select(r => new {
			//			r.Id,
			//			r.Name,
			//			r.Price
			//		}),
			//		Menu = new {
			//			d.Menu.Id,
			//			d.Menu.Code,
			//			d.Menu.Name,
			//			d.Menu.NameAbbr,
			//			d.Menu.Unit,
			//			d.Menu.IsSetMeal,
			//			Printer = new {
			//				d.Menu.Department.Printer.Id,
			//				d.Menu.Department.Printer.Name,
			//				d.Menu.Department.Printer.IpAddress,
			//				d.Menu.Department.Printer.Usable
			//			}
			//		}
			//	}),
			//	DinePaidDetails = p.DinePaidDetails.Select(d => new {
			//		d.Price,
			//		d.RecordId,
			//		PayKind = new {
			//			d.PayKind.Id,
			//			d.PayKind.Name,
			//			d.PayKind.Type
			//		}
			//	})
			//});
		}
	}
}