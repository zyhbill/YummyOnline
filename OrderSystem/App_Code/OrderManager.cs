using HotelDAO;
using HotelDAO.Models;
using OrderSystem.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using YummyOnlineDAO.Identity;
using YummyOnlineDAO.Models;
using Protocal;

namespace OrderSystem {
	public class OrderManager : BaseHotelManager {
		public OrderManager(string connString) : base(connString) { }

		public async Task<FunctionResult> CreateDine(Cart cart, CartAddition addition) {
			DinePaidDetail mainPaidDetail = new DinePaidDetail {
				PayKind = await ctx.PayKinds.FirstOrDefaultAsync(p => p.Id == cart.PayKindId),
				Price = 0,
			};
			Dine dine = new Dine {
				Type = DineType.ToStay,
				HeadCount = cart.HeadCount,
				IsOnline = mainPaidDetail.PayKind.Type == PayKindType.Online,
				IsPaid = false,
				Desk = await ctx.Desks.FirstOrDefaultAsync(p => p.Id == cart.DeskId),

				UserId = addition.UserId,
				WaiterId = addition.WaiterId,
				ClerkId = addition.ClerkId,

				DineMenus = new List<DineMenu>(),
				Remarks = new List<Remark>(),
				DinePaidDetails = new List<DinePaidDetail>()
			};

			// 订单备注
			cart.Remarks?.ForEach(r => {
				Remark remark = ctx.Remarks.FirstOrDefault(p => p.Id == r);
				dine.Price += remark.Price;
				dine.OriPrice += remark.Price;
				dine.Remarks.Add(remark);
			});

			// 是否有自定义折扣方案
			if(addition.Discount != null) {
				dine.Discount = (double)addition.Discount;
				dine.DiscountName = addition.DiscountName;
			}
			else {
				await handleDiscount(mainPaidDetail.PayKind, dine);
			}

			foreach(Cart.MenuExtension menuExtension in cart.OrderedMenus) {
				Menu menu = await ctx.Menus
					.Include(p => p.MenuPrice)
					.FirstOrDefaultAsync(p => p.Id == menuExtension.Id);
				DineMenu dineDetail = new DineMenu {
					Count = menuExtension.Ordered,
					OriPrice = menu.MenuPrice.Price,
					Price = menu.MenuPrice.Price,
					Menu = menu,
					Remarks = new List<Remark>()
				};

				// 是否排除在总单打折之外
				bool excludePayDiscount = menu.MenuPrice.ExcludePayDiscount;

				// 是否打折
				if(menu.MenuPrice.Discount < 1) {
					excludePayDiscount = true;
					dineDetail.Price = menu.MenuPrice.Price * (decimal)menu.MenuPrice.Discount;
				}
				// 是否为特价菜
				DayOfWeek week = DateTime.Now.DayOfWeek;
				MenuOnSale menuOnSales = await ctx.MenuOnSales.FirstOrDefaultAsync(p => p.Id == menu.Id && p.OnSaleWeek == week);
				if(menuOnSales != null) {
					excludePayDiscount = true;
					dineDetail.Price = menuOnSales.Price;
				}
				// 是否为套餐
				var menuSetMeals = await ctx.MenuSetMeals.FirstOrDefaultAsync(p => p.MenuSetId == menu.Id);
				if(menuSetMeals != null) {
					excludePayDiscount = true;
				}

				if(!excludePayDiscount) {
					dineDetail.Price = menu.MenuPrice.Price * (decimal)dine.Discount;
				}

				menuExtension.Remarks?.ForEach(r => {
					Remark remark = ctx.Remarks.FirstOrDefault(p => p.Id == r);

					dineDetail.RemarkPrice += remark.Price;
					dineDetail.Remarks.Add(remark);
				});

				dine.Price += dineDetail.Price * dineDetail.Count + dineDetail.RemarkPrice;
				dine.OriPrice += dineDetail.OriPrice * dineDetail.Count + dineDetail.RemarkPrice;

				dine.DineMenus.Add(dineDetail);

				menu.Ordered += dineDetail.Count;
			}
			mainPaidDetail.Price = dine.Price;

			if(!await handlePoints(cart.PriceInPoints, mainPaidDetail, dine)) {
				return new FunctionResult(false, "积分不足");
			}

			// 检测前端计算的金额与后台计算的金额是否相同，如果前端金额为null则检测
			if(cart.Price != null && Math.Abs(mainPaidDetail.Price - (decimal)cart.Price) > 0.01m) {
				ctx.Logs.Add(new HotelDAO.Models.Log {
					Level = HotelDAO.Models.Log.LogLevel.Error,
					Message = $"Price Error, Cart Price: {cart.Price}, Cal Price: {mainPaidDetail.Price}"
				});
				await ctx.SaveChangesAsync();
				return new FunctionResult(false, "金额有误");
			}

			// 如果是线上支付，则添加DinePaidDetail信息，否则不添加，交给收银系统处理
			if(mainPaidDetail.PayKind.Type == PayKindType.Online) {
				dine.DinePaidDetails.Add(mainPaidDetail);
			}

			// 订单发票
			if(cart.Invoice != null) {
				dine.Invoice = cart.Invoice;
			}

			ctx.Dines.Add(dine);

			await ctx.SaveChangesAsync();

			return new FunctionResult(true, dine);
		}

		/// <summary>
		/// 整单打折处理
		/// </summary>
		/// <param name="payKind"></param>
		/// <param name="dine"></param>
		/// <returns></returns>
		private async Task handleDiscount(PayKind payKind, Dine dine) {
			double minDiscount = 1;
			string minDiscountName = null;

			if(payKind.Discount < minDiscount) {
				minDiscount = payKind.Discount;
				minDiscountName = payKind.Name + "折扣";
			}

			DayOfWeek week = DateTime.Now.DayOfWeek;
			List<TimeDiscount> timeDicsounts = await ctx.TimeDiscounts.Where(p => p.Week == week).ToListAsync();
			TimeSpan now = DateTime.Now.TimeOfDay;
			foreach(TimeDiscount timeDiscount in timeDicsounts) {
				if(now >= timeDiscount.From && now <= timeDiscount.To) {
					if(timeDiscount.Discount < minDiscount) {
						minDiscount = timeDiscount.Discount;
						minDiscountName = timeDiscount.Name;
					}
					break;
				}
			}

			Customer customer = await ctx.Customers.FirstOrDefaultAsync(p => p.Id == dine.UserId);
			if(customer != null) {
				VipDiscount vipDiscounts = await ctx.VipDiscounts.FirstOrDefaultAsync(p => p.Level.Id == customer.VipLevelId);
				if(vipDiscounts != null) {
					if(vipDiscounts.Discount < minDiscount) {
						minDiscount = vipDiscounts.Discount;
						minDiscountName = vipDiscounts.Name;
					}
				}
			}

			dine.Discount = minDiscount;
			dine.DiscountName = minDiscountName;
		}

		/// <summary>
		/// 积分处理
		/// </summary>
		private async Task<bool> handlePoints(decimal priceInPoints, DinePaidDetail mainPaidDetail, Dine dine) {
			if(priceInPoints != 0) {
				HotelConfig hotelConfig = await ctx.HotelConfigs.FirstOrDefaultAsync();

				DinePaidDetail pointsPaidDetail = new DinePaidDetail {
					PayKind = await ctx.PayKinds.FirstOrDefaultAsync(p => p.Type == PayKindType.Points),
					Price = priceInPoints
				};

				Customer customer = await ctx.Customers.FirstOrDefaultAsync(p => p.Id == dine.UserId);
				int customerPoints = customer == null ? 0 : customer.Points;

				if(pointsPaidDetail.Price > customerPoints / hotelConfig.PointsRatio) {
					return false;
				}
				if(pointsPaidDetail.Price > dine.Price) {
					pointsPaidDetail.Price = dine.Price;
				}

				mainPaidDetail.Price -= pointsPaidDetail.Price;

				dine.DinePaidDetails.Add(pointsPaidDetail);
			}
			return true;
		}

		public async Task<bool> OfflinePayCompleted(WaiterPaidDetails paidDetails) {
			Dine dine = await ctx.Dines.FirstOrDefaultAsync(p => p.Id == paidDetails.DineId);
			List<DinePaidDetail> exisedPaidDetails = await ctx.DinePaidDetails.Where(p => p.Dine.Id == paidDetails.DineId).ToListAsync();

			decimal price = 0;
			foreach(var p in exisedPaidDetails) {
				price += p.Price;
			}
			foreach(var p in paidDetails.PaidDetails) {
				price += p.Price;
				ctx.DinePaidDetails.Add(new DinePaidDetail {
					DineId = paidDetails.DineId,
					PayKindId = p.PayKindId,
					Price = p.Price,
					RecordId = p.RecordId
				});
			}
			if(Math.Abs(dine.Price - price) > 0.01m) {
				return false;
			}
			dine.IsPaid = true;

			await changeCustomerPoints(dine);

			await ctx.SaveChangesAsync();

			return true;
		}

		public async Task OnlinePayCompleted(string dineId, string recordId) {
			Dine dine = await ctx.Dines.FirstOrDefaultAsync(p => p.Id == dineId);
			dine.IsPaid = true;

			DinePaidDetail mainPaidDetail = await ctx.DinePaidDetails.FirstOrDefaultAsync(p => p.Dine.Id == dineId && p.PayKind.Type == PayKindType.Online);
			mainPaidDetail.RecordId = recordId;

			await changeCustomerPoints(dine);

			await ctx.SaveChangesAsync();
		}
		private async Task changeCustomerPoints(Dine dine) {
			DinePaidDetail pointsPaidDetail = await ctx.DinePaidDetails.FirstOrDefaultAsync(p => p.Dine.Id == dine.Id && p.PayKind.Type == PayKindType.Points);

			Customer customer = await ctx.Customers.FirstOrDefaultAsync(p => p.Id == dine.UserId);

			if(pointsPaidDetail != null) {
				HotelConfig config = await ctx.HotelConfigs.FirstOrDefaultAsync();
				customer.Points -= Convert.ToInt32((double)pointsPaidDetail.Price / config.PointsRatio);
			}
			if(!await new UserManager().IsInRoleAsync(dine.UserId, Role.Nemo)) {
				List<DineMenu> dineMenus = await ctx.DineMenus.Include(p => p.Menu.MenuPrice).Where(p => p.Dine.Id == dine.Id).ToListAsync();
				dineMenus?.ForEach(m => {
					customer.Points += m.Menu.MenuPrice.Points * m.Count;
				});
			}
		}

		public async Task PrintCompleted(string dineId) {
			Dine dine = await ctx.Dines.FirstOrDefaultAsync(p => p.Id == dineId);
			dine.Status = DineStatus.Printed;
			await ctx.SaveChangesAsync();
		}
	}
}