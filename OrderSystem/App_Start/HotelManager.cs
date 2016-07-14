using HotelDAO.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Threading.Tasks;

namespace OrderSystem {
	public partial class HotelManager : HotelDAO.BaseHotelManager {
		public HotelManager(string connString) : base(connString) { }

		public async Task<Desk> GetDeskByQrCode(string qrCode) {
			return await ctx.Desks.FirstOrDefaultAsync(p => p.QrCode == qrCode);
		}

		public async Task<PayKind> GetPayKindById(int payKindId) {
			return await ctx.PayKinds.FirstOrDefaultAsync(p => p.Id == payKindId);
		}

		public async Task<Customer> GetCustomer(string userId) {
			return await ctx.Customers.Include(p => p.VipLevel).FirstOrDefaultAsync(p => p.Id == userId);
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

		public async Task<bool> IsDinePaid(string dineId) {
			bool isPaid = await ctx.Dines.Where(p => p.Id == dineId).Select(p => p.IsPaid).FirstOrDefaultAsync();
			return isPaid;
		}
		public async Task<int> GetHistoryDinesCount(string userId) {
			return await ctx.Dines.CountAsync(p => p.UserId == userId);
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
	}

	// 打印协议相关
	public partial class HotelManager {
		public async Task<Protocol.PrintingProtocol.PrinterFormat> GetPrinterFormatForPrinting() {
			return await ctx.PrinterFormats.Select(p => new Protocol.PrintingProtocol.PrinterFormat {
				Font = p.Font,
				PaperSize = p.PaperSize,
				ReciptBigFontSize = p.ReciptBigFontSize,
				ReciptFontSize = p.ReciptFontSize,
				ReciptSmallFontSize = p.ReciptSmallFontSize,
				ServeOrderFontSize = p.ServeOrderFontSize,
				ServeOrderSmallFontSize = p.ServeOrderSmallFontSize,
				KitchenOrderFontSize = p.KitchenOrderFontSize,
				KitchenOrderSmallFontSize = p.KitchenOrderSmallFontSize,
				ShiftBigFontSize = p.ShiftBigFontSize,
				ShiftFontSize = p.ShiftFontSize,
				ShiftSmallFontSize = p.ShiftSmallFontSize,
			}).FirstOrDefaultAsync();
		}
		public async Task<Protocol.PrintingProtocol.Dine> GetDineForPrintingById(string dineId, List<int> dineMenuIds = null) {
			var dine = await formatDineForPrinting(ctx.Dines
					.Where(p => p.Id == dineId))
					.FirstOrDefaultAsync();

			if(dineMenuIds != null && dineMenuIds.Count > 0) {
				dine.DineMenus.RemoveAll(p => !dineMenuIds.Contains(p.Id));
			}

			foreach(var dineMenu in dine.DineMenus) {
				if(dineMenu.Menu.IsSetMeal) {
					dineMenu.Menu.SetMealMenus = await ctx.MenuSetMeals
					.Where(p => p.MenuSetId == dineMenu.Menu.Id)
					.Select(p => new Protocol.PrintingProtocol.SetMealMenu {
						Id = p.Menu.Id,
						Name = p.Menu.Name,
						Count = p.Count
					}).ToListAsync();
				}
			}
			return dine;
		}

		private IQueryable<Protocol.PrintingProtocol.Dine> formatDineForPrinting(IQueryable<Dine> dine) {
			return dine.Select(p => new Protocol.PrintingProtocol.Dine {
				Id = p.Id,
				Status = p.Status,
				Type = p.Type,
				HeadCount = p.HeadCount,
				Price = p.Price,
				OriPrice = p.OriPrice,
				Change = p.Change,
				Discount = p.Discount,
				DiscountName = p.DiscountName,
				DiscountType = p.DiscountType,
				BeginTime = p.BeginTime,
				IsPaid = p.IsPaid,
				IsOnline = p.IsOnline,
				UserId = p.UserId,
				Waiter = new Protocol.PrintingProtocol.Staff {
					Id = p.Waiter.Id,
					Name = p.Waiter.Name
				},
				Clerk = new Protocol.PrintingProtocol.Staff {
					Id = p.Clerk.Id,
					Name = p.Clerk.Name
				},
				Remarks = p.Remarks.Select(pp => new Protocol.PrintingProtocol.Remark {
					Id = pp.Id,
					Name = pp.Name,
					Price = pp.Price
				}).ToList(),
				Desk = new Protocol.PrintingProtocol.Desk {
					Id = p.Desk.Id,
					QrCode = p.Desk.QrCode,
					Name = p.Desk.Name,
					Description = p.Desk.Description,
					AreaType = p.Desk.Area.Type,
					ReciptPrinter = new Protocol.PrintingProtocol.Printer {
						Id = p.Desk.Area.DepartmentRecipt.Printer.Id,
						Name = p.Desk.Area.DepartmentRecipt.Printer.Name,
						IpAddress = p.Desk.Area.DepartmentRecipt.Printer.IpAddress,
						Usable = p.Desk.Area.DepartmentRecipt.Printer.Usable
					},
					ServePrinter = new Protocol.PrintingProtocol.Printer {
						Id = p.Desk.Area.DepartmentServe.Printer.Id,
						Name = p.Desk.Area.DepartmentServe.Printer.Name,
						IpAddress = p.Desk.Area.DepartmentServe.Printer.IpAddress,
						Usable = p.Desk.Area.DepartmentServe.Printer.Usable
					}
				},
				DineMenus = p.DineMenus.Select(d => new Protocol.PrintingProtocol.DineMenu {
					Id = d.Id,
					Status = d.Status,
					Count = d.Count,
					OriPrice = d.OriPrice,
					Price = d.Price,
					RemarkPrice = d.RemarkPrice,
					Remarks = d.Remarks.Select(r => new Protocol.PrintingProtocol.Remark {
						Id = r.Id,
						Name = r.Name,
						Price = r.Price
					}).ToList(),
					Menu = new Protocol.PrintingProtocol.Menu {
						Id = d.Menu.Id,
						Code = d.Menu.Code,
						Name = d.Menu.Name,
						NameAbbr = d.Menu.NameAbbr,
						Unit = d.Menu.Unit,
						IsSetMeal = d.Menu.IsSetMeal,
						Printer = new Protocol.PrintingProtocol.Printer {
							Id = d.Menu.Department.Printer.Id,
							Name = d.Menu.Department.Printer.Name,
							IpAddress = d.Menu.Department.Printer.IpAddress,
							Usable = d.Menu.Department.Printer.Usable
						}
					},
					ReturnedWaiter = new Protocol.PrintingProtocol.Staff {
						Id = d.ReturnedWaiter.Id,
						Name = d.ReturnedWaiter.Name
					},
					ReturnedReason = d.ReturnedReason
				}).ToList(),
				DinePaidDetails = p.DinePaidDetails.Select(d => new Protocol.PrintingProtocol.DinePaidDetail {
					Price = d.Price,
					RecordId = d.RecordId,
					PayKind = new Protocol.PrintingProtocol.PayKind {
						Id = d.PayKind.Id,
						Name = d.PayKind.Name,
						Type = d.PayKind.Type
					}
				}).ToList(),
				TakeOut = new Protocol.PrintingProtocol.TakeOut {
					Address = p.TakeOut.Address
				}
			});
		}


		public async Task<string> GetShiftPrinterName() {
			return await ctx.HotelConfigs.Select(p => p.ShiftPrinter.Name).FirstOrDefaultAsync();
		}
		public async Task<List<Protocol.PrintingProtocol.Shift>> GetShiftsForPrinting(List<int> ids, DateTime dateTime) {
			var dbShifts = await ctx.Shifts.Where(p => ids.Contains(p.Id) && SqlFunctions.DateDiff("day", p.DateTime, dateTime) == 0)
				.Select(p => new {
					p.Id,
					p.DateTime,
					p.PayKind.Name,
					p.ReceivablePrice,
					p.RealPrice
				})
				.ToListAsync();

			List<Protocol.PrintingProtocol.Shift> shifts = new List<Protocol.PrintingProtocol.Shift>();

			while(dbShifts.Count != 0) {
				Protocol.PrintingProtocol.Shift shift = new Protocol.PrintingProtocol.Shift {
					Id = dbShifts[0].Id,
					DateTime = dbShifts[0].DateTime,
					ShiftDetails = new List<Protocol.PrintingProtocol.ShiftDetail>()
				};
				shift.ShiftDetails = dbShifts
					.Where(p => p.Id == shift.Id && p.DateTime == shift.DateTime)
					.Select(p => new Protocol.PrintingProtocol.ShiftDetail {
						PayKind = p.Name,
						RealPrice = p.RealPrice,
						ReceivablePrice = p.ReceivablePrice
					}).ToList();

				dbShifts.RemoveAll(p => p.Id == shift.Id && p.DateTime == shift.DateTime);

				shifts.Add(shift);
			}

			return shifts;
		}
	}
}