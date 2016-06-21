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
		public async Task<Protocal.DineForPrintingProtocal.PrinterFormat> GetPrinterFormatForPrinting() {
			return await ctx.PrinterFormats.Select(p => new Protocal.DineForPrintingProtocal.PrinterFormat {
				Font = p.Font,
				PaperSize = p.PaperSize,
				ReciptBigFontSize = p.ReciptBigFontSize,
				ReciptFontSize = p.ReciptFontSize,
				ReciptSmallFontSize = p.ReciptSmallFontSize,
				ServeOrderFontSize = p.ServeOrderFontSize,
				ServeOrderSmallFontSize = p.ServeOrderSmallFontSize,
				KitchenOrderFontSize = p.KitchenOrderFontSize,
				KitchenOrderSmallFontSize = p.KitchenOrderSmallFontSize
			}).FirstOrDefaultAsync();
		}
		public async Task<Protocal.DineForPrintingProtocal.Dine> GetDineForPrintingById(string dineId, List<int> dineMenuIds = null) {
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
					.Select(p => new Protocal.DineForPrintingProtocal.SetMealMenu {
						Id = p.Menu.Id,
						Name = p.Menu.Name,
						Count = p.Count
					}).ToListAsync();
				}
			}
			return dine;
		}

		private IQueryable<Protocal.DineForPrintingProtocal.Dine> formatDineForPrinting(IQueryable<Dine> dine) {
			return dine.Select(p => new Protocal.DineForPrintingProtocal.Dine {
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
				Waiter = new Protocal.DineForPrintingProtocal.Staff {
					Id = p.Waiter.Id,
					Name = p.Waiter.Name
				},
				Clerk = new Protocal.DineForPrintingProtocal.Staff {
					Id = p.Clerk.Id,
					Name = p.Clerk.Name
				},
				Remarks = p.Remarks.Select(pp => new Protocal.DineForPrintingProtocal.Remark {
					Id = pp.Id,
					Name = pp.Name,
					Price = pp.Price
				}).ToList(),
				Desk = new Protocal.DineForPrintingProtocal.Desk {
					Id = p.Desk.Id,
					QrCode = p.Desk.QrCode,
					Name = p.Desk.Name,
					Description = p.Desk.Description,
					ReciptPrinter = new Protocal.DineForPrintingProtocal.Printer {
						Id = p.Desk.Area.DepartmentRecipt.Printer.Id,
						Name = p.Desk.Area.DepartmentRecipt.Printer.Name,
						IpAddress = p.Desk.Area.DepartmentRecipt.Printer.IpAddress,
						Usable = p.Desk.Area.DepartmentRecipt.Printer.Usable
					},
					ServePrinter = new Protocal.DineForPrintingProtocal.Printer {
						Id = p.Desk.Area.DepartmentServe.Printer.Id,
						Name = p.Desk.Area.DepartmentServe.Printer.Name,
						IpAddress = p.Desk.Area.DepartmentServe.Printer.IpAddress,
						Usable = p.Desk.Area.DepartmentServe.Printer.Usable
					}
				},
				DineMenus = p.DineMenus.Select(d => new Protocal.DineForPrintingProtocal.DineMenu {
					Id = d.Id,
					Status = d.Status,
					Count = d.Count,
					OriPrice = d.OriPrice,
					Price = d.Price,
					RemarkPrice = d.RemarkPrice,
					Remarks = d.Remarks.Select(r => new Protocal.DineForPrintingProtocal.Remark {
						Id = r.Id,
						Name = r.Name,
						Price = r.Price
					}).ToList(),
					Menu = new Protocal.DineForPrintingProtocal.Menu {
						Id = d.Menu.Id,
						Code = d.Menu.Code,
						Name = d.Menu.Name,
						NameAbbr = d.Menu.NameAbbr,
						Unit = d.Menu.Unit,
						IsSetMeal = d.Menu.IsSetMeal,
						Printer = new Protocal.DineForPrintingProtocal.Printer {
							Id = d.Menu.Department.Printer.Id,
							Name = d.Menu.Department.Printer.Name,
							IpAddress = d.Menu.Department.Printer.IpAddress,
							Usable = d.Menu.Department.Printer.Usable
						}
					},
					ReturnedWaiter = new Protocal.DineForPrintingProtocal.Staff {
						Id = d.ReturnedWaiter.Id,
						Name = d.ReturnedWaiter.Name
					},
					ReturnedReason = d.ReturnedReason
				}).ToList(),
				DinePaidDetails = p.DinePaidDetails.Select(d => new Protocal.DineForPrintingProtocal.DinePaidDetail {
					Price = d.Price,
					RecordId = d.RecordId,
					PayKind = new Protocal.DineForPrintingProtocal.PayKind {
						Id = d.PayKind.Id,
						Name = d.PayKind.Name,
						Type = d.PayKind.Type
					}
				}).ToList()
			});
		}
	}
}