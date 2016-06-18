using HotelDAO.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelDAO {
	public partial class HotelManager {
		public async Task InitializeHotel(int hotelId, string adminId) {
			ctx.HotelConfigs.Add(new HotelConfig {
				Id = hotelId
			});
			await ctx.SaveChangesAsync();

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
			await ctx.SaveChangesAsync();

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
			await ctx.SaveChangesAsync();
		}
	}
}