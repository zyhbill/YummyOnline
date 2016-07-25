using HotelDAO.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Threading.Tasks;

namespace YummyOnline {
	public class HotelManager : HotelDAO.BaseHotelManager {
		public HotelManager(string connString) : base(connString) { }

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

		public async Task<List<string>> GetAllDineIds(DateTime dateTime) {
			return await ctx.Dines.Where(p => SqlFunctions.DateDiff("day", p.BeginTime, dateTime) == 0).Select(p => p.Id).ToListAsync();
		}
		public async Task<int> GetDineCount(DateTime dateTime) {
			return await ctx.Dines.CountAsync(p => SqlFunctions.DateDiff("day", p.BeginTime, dateTime) == 0);
		}
		public async Task<int> GetDineCount(string userId) {
			return await ctx.Dines.Where(p => p.UserId == userId).CountAsync();
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

		public async Task InitializeHotel(int hotelId, string adminId) {
			ctx.HotelConfigs.Add(new HotelConfig {
				Id = hotelId
			});

			ctx.PayKinds.Add(new PayKind {
				Name = "现金",
				Type = PayKindType.Cash,
				Usable = true,
				Discount = 1
			});
			ctx.PayKinds.Add(new PayKind {
				Name = "微信支付",
				Type = PayKindType.Online,
				Usable = true,
				Discount = 1,
				RedirectUrl = "/WeixinTest",
				CompleteUrl = "/Payment/Complete",
				NotifyUrl = "/Payment/OnlineNotify"
			});
			ctx.PayKinds.Add(new PayKind {
				Name = "积分抵扣",
				Type = PayKindType.Points,
				Usable = false,
				Discount = 1
			});
			ctx.PayKinds.Add(new PayKind {
				Name = "其他支付",
				Type = PayKindType.Other,
				Usable = true,
				Discount = 1,
				CompleteUrl = "/Payment/Complete"
			});
			ctx.PayKinds.Add(new PayKind {
				Name = "随机立减",
				Type = PayKindType.RandomPreference,
				Usable = true,
				Discount = 1,
			});

			Staff staff = new Staff {
				Id = adminId,
				Name = "管理员",
				StaffRoles = new List<StaffRole>(),
			};
			StaffRole role = new StaffRole {
				Name = "管理员组",
				Schemas = new List<StaffRoleSchema>()
			};
			staff.StaffRoles.Add(role);
			foreach(Schema item in Enum.GetValues(typeof(Schema))) {
				role.Schemas.Add(new StaffRoleSchema {
					StaffRole = role,
					Schema = item
				});
			}
			ctx.Staffs.Add(staff);

			ctx.PrinterFormats.Add(new PrinterFormat {
				PaperSize = 556,
				Font = "宋体",
				ColorDepth = 200,
				ShiftBigFontSize = 25,
				ShiftFontSize = 17,
				ShiftSmallFontSize = 15,
				ReciptBigFontSize = 25,
				ReciptFontSize = 17,
				ReciptSmallFontSize = 15,
				ServeOrderFontSize = 19,
				ServeOrderSmallFontSize = 19,
				KitchenOrderFontSize = 19,
				KitchenOrderSmallFontSize = 19
			});
			await ctx.SaveChangesAsync();
		}
	}
}