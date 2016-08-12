using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using HotelDAO.Models;
using HotelDAO;
namespace Management
{
    public class HotelManager:HotelDAO.BaseHotelManager
    {
        public HotelManager(string ConnectingStr) : base(ConnectingStr) { }
        public async Task<List<Desk>> GetTakeOutDeskes()
        {
            return await ctx.Desks
                .Include(d => d.Area)
                .Where(d => d.Area.Type == AreaType.TakeOut&&d.Usable==true)
                .ToListAsync();
        }

        public async Task<List<Dine>> GetDines(List<string> Ids)
        {
            if (Ids.Count > 0)
            {
                return await ctx.Dines.Where(d => Ids.Contains(d.Id)).ToListAsync();
            }
            else
            {
                return  null;
            }
        }
        public async Task<List<PayKind>> GetOfflinePayKinds()
        {
            return await ctx.PayKinds.Where(d => (d.Type == PayKindType.Cash || d.Type == PayKindType.Offline)&&d.Usable==true).ToListAsync();
        }
        public async Task<int> GetCashId()
        {
            return await ctx.PayKinds.Where(d => d.Type == PayKindType.Cash).Select(d => d.Id).FirstOrDefaultAsync();
        }

        public async void ManageLog(string Message,Log.LogLevel Level = Log.LogLevel.Success,string Detail=null)
        {
            ctx.Logs.Add(new Log
            {
                DateTime = DateTime.Now,
                Detail = Detail,
                Message = Message,
                Level = Level
            });
            await ctx.SaveChangesAsync();
        }

        public async Task ChangeShow (string Id)
        {
            var Class = await ctx.MenuClasses.FirstOrDefaultAsync(d => d.Id == Id);
            Class.IsShow = !Class.IsShow;
            await ctx.SaveChangesAsync();
        }
    }
}
