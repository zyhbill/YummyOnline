﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Threading.Tasks;
using YummyOnlineDAO.Models;
using Utility;

namespace YummyOnline {
	public partial class YummyOnlineManager : YummyOnlineDAO.YummyOnlineManager {

		#region 系统相关
		public async Task<bool> IsInNewDineInformClientGuids(Guid guid) {
			NewDineInformClientGuid clientGuid = await ctx.NewDineInformClientGuids.FirstOrDefaultAsync(p => p.Guid == guid);
			return clientGuid != null;
		}

		public async Task<bool> AddGuid(NewDineInformClientGuid clientGuid) {
			ctx.NewDineInformClientGuids.Add(clientGuid);
			int result = await ctx.SaveChangesAsync();
			return result == 1;
		}
		public async Task<bool> DeleteGuid(Guid guid) {
			NewDineInformClientGuid clientGuid = await ctx.NewDineInformClientGuids.FirstOrDefaultAsync(p => p.Guid == guid);
			if(clientGuid == null) {
				return false;
			}
			ctx.NewDineInformClientGuids.Remove(clientGuid);
			await ctx.SaveChangesAsync();
			return true;
		}
		#endregion

		#region 日志相关
		public async Task<dynamic> GetLogs(Log.LogProgram program, DateTime date, int? count) {
			IQueryable<Log> linq = ctx.Logs.Where(p => p.Program == program && SqlFunctions.DateDiff("day", p.DateTime, date) == 0)
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
		#endregion

		#region 订单相关
		public async Task<List<dynamic>> GetDinesByUserId(string userId) {
			List<dynamic> allDines = new List<dynamic>();

			List<Hotel> hotels = await GetHotels();
			foreach(Hotel h in hotels) {
				List<dynamic> dines = await new HotelManager(h.ConnectionString).GetFormatedHistoryDines(userId);
				if(dines.Count > 0) {
					allDines.Add(new {
						Hotel = new {
							h.Id,
							h.Name
						},
						Dines = dines
					});
				}
			}

			return allDines;
		}
		public async Task<int> GetDineCount() {
			int dineCount = 0;
			List<Hotel> hotels = await GetHotels();
			foreach(Hotel h in hotels) {
				HotelManager hotelManager = new HotelManager(h.ConnectionString);
				dineCount += await hotelManager.GetDineCount(DateTime.Now);
			}
			return dineCount;
		}
		public async Task<List<dynamic>> GetDinePerHourCount(DateTime dateTime) {
			List<dynamic> list = new List<dynamic>();
			List<Hotel> hotels = await GetHotels();
			foreach(Hotel h in hotels) {
				HotelManager hotelManager = new HotelManager(h.ConnectionString);

				list.Add(new {
					HotelName = h.Name,
					Counts = await hotelManager.GetDinePerHourCount(dateTime)
				});
			}
			return list;
		}
		#endregion

		#region 饭店相关
		public async Task<Hotel> GetFirstHotel() {
			return await ctx.Hotels.FirstOrDefaultAsync();
		}

		public async Task<List<Hotel>> GetHotelReadyForConfirms() {
			return await ctx.Hotels.Where(p => p.ConnectionString == null).ToListAsync();
		}
		public async Task<int> GetHotelCount() {
			return await ctx.Hotels.CountAsync();
		}
		public async Task UpdateHotel(Hotel hotel) {
			Hotel currHotel = await ctx.Hotels.FirstOrDefaultAsync(p => p.Id == hotel.Id);
			currHotel.CssThemePath = hotel.CssThemePath;
			currHotel.ConnectionString = hotel.ConnectionString;
			currHotel.AdminConnectionString = hotel.AdminConnectionString;
			currHotel.Usable = hotel.Usable;
			await ctx.SaveChangesAsync();
		}

		public async Task CreateHotel(int hotelId, string databaseName) {
			// 总数据库中增加连接字符串

			SystemConfig config = await GetSystemConfig();
			string connectionString = config.DefaultConnectionString.Replace("@@databaseName", databaseName);
			string adminConnectionString = config.DefaultAdminConnectionString.Replace("@@databaseName", databaseName);
			Hotel hotel = await ctx.Hotels.FirstOrDefaultAsync(p => p.Id == hotelId);
			hotel.ConnectionString = connectionString;
			hotel.AdminConnectionString = adminConnectionString;
			await ctx.SaveChangesAsync();
		}
		#endregion

		#region 用户相关
		public async Task<dynamic> GetUserById(string userId) {
			return await ctx.Users
				.Where(p => p.Id == userId)
				.Select(p => new {
					p.Id,
					p.CreateDate,
					p.UserName,
					p.PhoneNumber,
					p.Email
				}).FirstOrDefaultAsync();
		}
		public async Task<dynamic> GetUsers(Role role, int countPerPage = 0, int currPage = 0, bool withDineCount = false) {

			IQueryable<UserRole> linq = ctx.UserRoles.Where(p => p.Role == role).OrderByDescending(p => p.UserId);

			if(countPerPage != 0) {
				linq = linq.Skip(countPerPage * (currPage - 1)).Take(countPerPage);
			}

			var users = await linq.Select(p => new {
				p.User.Id,
				p.User.CreateDate,
				p.User.UserName,
				p.User.PhoneNumber,
				p.User.Email,
			}).ToListAsync();

			if(!withDineCount) {
				return users;
			}

			Dictionary<string, int> userDineCounts = new Dictionary<string, int>();

			List<Hotel> hotels = await GetHotels();
			foreach(var h in hotels) {
				HotelManager hotelManager = new HotelManager(h.ConnectionString);
				foreach(var user in users) {
					if(!userDineCounts.ContainsKey(user.Id)) {
						userDineCounts[user.Id] = await hotelManager.GetDineCount(user.Id);
					}
					else {
						userDineCounts[user.Id] += await hotelManager.GetDineCount(user.Id);
					}
				}
			}

			List<dynamic> userWithDineCounts = new List<dynamic>();

			foreach(var user in users) {
				userWithDineCounts.Add(DynamicsCombination.CombineDynamics(user, new {
					DineCount = userDineCounts[user.Id]
				}));
			}

			return userWithDineCounts;
		}
		public async Task<int> GetUserCount(Role role) {
			return await ctx.UserRoles.CountAsync(p => p.Role == role);
		}
		public async Task<List<dynamic>> GetUserDailyCount(Role role) {
			List<dynamic> list = new List<dynamic>();
			for(int i = -30; i <= 0; i++) {
				DateTime t = DateTime.Now.AddDays(i);
				int count = await ctx.UserRoles.CountAsync(p => p.Role == role && SqlFunctions.DateDiff("day", p.User.CreateDate, t) == 0);
				list.Add(new {
					DateTime = t,
					Count = count
				});
			}
			return list;
		}
		/// <summary>
		/// 删除不是当天的未点单的匿名用户
		/// </summary>
		/// <returns></returns>
		public async Task DeleteNemoesHavenotDine() {
			List<User> nemoes = await ctx.UserRoles
				.Where(p => p.Role == Role.Nemo && SqlFunctions.DateDiff("day", p.User.CreateDate, DateTime.Now) != 0)
				.Select(p => p.User)
				.ToListAsync();

			Dictionary<string, int> userDineCounts = new Dictionary<string, int>();

			List<Hotel> hotels = await GetHotels();
			foreach(var h in hotels) {
				HotelManager hotelManager = new HotelManager(h.ConnectionString);
				foreach(User nemo in nemoes) {
					if(!userDineCounts.ContainsKey(nemo.Id)) {
						userDineCounts[nemo.Id] = await hotelManager.GetDineCount(nemo.Id);
					}
					else {
						userDineCounts[nemo.Id] += await hotelManager.GetDineCount(nemo.Id);
					}
				}
			}

			foreach(User nemo in nemoes) {
				if(userDineCounts[nemo.Id] == 0) {
					ctx.Users.Remove(nemo);
				}
			}
			await ctx.SaveChangesAsync();
		}

		public async Task<string> GetHotelAdminId(int hotelId) {
			return await ctx.Staffs.Where(p => p.HotelId == hotelId && p.IsHotelAdmin).Select(p => p.Id).FirstOrDefaultAsync();
		}
		public async Task<dynamic> GetHotelAdmins() {
			return await ctx.Staffs.Where(p => p.IsHotelAdmin).Select(p => new {
				p.Id,
				p.SigninName,
				p.PhoneNumber,
				p.Email,
				p.CreateDate,
				StaffCount = p.Hotel.Staffs.Count,
				Hotel = new {
					p.Hotel.Id,
					p.Hotel.Name
				}
			}).ToListAsync();
		}
		#endregion

		public async Task<dynamic> GetArticles(int hotelId) {
			return await ctx.Articles.Where(p => p.HotelId == hotelId).Select(p => new {
				p.Id,
				p.Title,
				p.Description,
				p.PicturePath,
				p.Body,
				p.DateTime,
				p.Status
			}).ToListAsync();
		}

	}
}
