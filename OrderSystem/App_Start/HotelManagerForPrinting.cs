using HotelDAO.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Threading.Tasks;

namespace OrderSystem {
	// 打印协议相关
	public partial class HotelManager {
		public async Task<Protocol.PrintingProtocol.PrinterFormat> GetPrinterFormatForPrinting() {
			return await ctx.PrinterFormats.Select(p => new Protocol.PrintingProtocol.PrinterFormat {
				PaperSize = p.PaperSize,
				Font = p.Font,
				ColorDepth = p.ColorDepth,
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
				PaddingRight = p.PaddingRight,
			}).FirstOrDefaultAsync();
		}
		public async Task<Protocol.PrintingProtocol.Dine> GetDineForPrintingById(string dineId, List<int> dineMenuIds = null) {
			var dine = await formatDineForPrinting(ctx.Dines
					.Where(p => p.Id == dineId))
					.FirstOrDefaultAsync();

			if(dineMenuIds != null && dineMenuIds.Count > 0) {
				dine.DineMenus.RemoveAll(p => !dineMenuIds.Contains(p.Id));
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
					ReciptPrinter = p.Desk.Area.DepartmentRecipt.Printer == null ? null : new Protocol.PrintingProtocol.Printer {
						Id = p.Desk.Area.DepartmentRecipt.Printer.Id,
						Name = p.Desk.Area.DepartmentRecipt.Printer.Name,
						IpAddress = p.Desk.Area.DepartmentRecipt.Printer.IpAddress,
						Usable = p.Desk.Area.DepartmentRecipt.Printer.Usable
					},
					ServePrinter = p.Desk.Area.DepartmentServe.Printer == null ? null : new Protocol.PrintingProtocol.Printer {
						Id = p.Desk.Area.DepartmentServe.Printer.Id,
						Name = p.Desk.Area.DepartmentServe.Printer.Name,
						IpAddress = p.Desk.Area.DepartmentServe.Printer.IpAddress,
						Usable = p.Desk.Area.DepartmentServe.Printer.Usable
					},
					ReciptDepartmentName = p.Desk.Area.DepartmentRecipt.Name,
					ServeDepartmentName = p.Desk.Area.DepartmentServe.Name
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
						Printer = d.Menu.Department.Printer == null ? null : new Protocol.PrintingProtocol.Printer {
							Id = d.Menu.Department.Printer.Id,
							Name = d.Menu.Department.Printer.Name,
							IpAddress = d.Menu.Department.Printer.IpAddress,
							Usable = d.Menu.Department.Printer.Usable
						},
						DepartmentName = d.Menu.Department.Name
					},
					ReturnedWaiter = new Protocol.PrintingProtocol.Staff {
						Id = d.ReturnedWaiter.Id,
						Name = d.ReturnedWaiter.Name
					},
					ReturnedReason = d.ReturnedReason,
					SetMealClasses = d.SetMeals.GroupBy(s => s.ClassName).Select(s => new Protocol.PrintingProtocol.DineMenuSetMealClass {
						ClassName = s.Key,
						SetMealMenus = s.Select(m => new Protocol.PrintingProtocol.DineMenuSetMealMenu {
							Count = m.Count,
							Menu = new Protocol.PrintingProtocol.Menu {
								Id = m.Menu.Id,
								Code = m.Menu.Code,
								Name = m.Menu.Name,
								NameAbbr = m.Menu.NameAbbr,
								Unit = m.Menu.Unit,
								IsSetMeal = m.Menu.IsSetMeal,
								Printer = m.Menu.Department.Printer == null ? null : new Protocol.PrintingProtocol.Printer {
									Id = m.Menu.Department.Printer.Id,
									Name = m.Menu.Department.Printer.Name,
									IpAddress = m.Menu.Department.Printer.IpAddress,
									Usable = m.Menu.Department.Printer.Usable
								},
								DepartmentName = m.Menu.Department.Name
							}
						}).ToList()
					}).ToList(),
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
					Address = p.TakeOut.Address,
					RecordId = p.TakeOut.RecordId
				}
			});
		}


		public async Task<Protocol.PrintingProtocol.Printer> GetShiftPrinter() {
			return await ctx.HotelConfigs.Select(p => new Protocol.PrintingProtocol.Printer {
				Id = p.ShiftPrinter.Id,
				Name = p.ShiftPrinter.Name,
				IpAddress = p.ShiftPrinter.IpAddress,
				Usable = p.ShiftPrinter.Usable
			}).FirstOrDefaultAsync();
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

		public async Task<List<Protocol.PrintingProtocol.Printer>> GetPrinters() {
			return await ctx.Printers.Where(p => p.Usable).Select(p => new Protocol.PrintingProtocol.Printer {
				Id = p.Id,
				Name = p.Name,
				IpAddress = p.IpAddress,
				Usable = p.Usable
			}).ToListAsync();
		}
	}
}