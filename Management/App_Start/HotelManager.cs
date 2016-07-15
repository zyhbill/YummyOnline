using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace Management
{
    public class HotelManager:YummyOnlineDAO.YummyOnlineManager
    {
        public async Task<List<string>> GetTakeOutDeskes()
        {
            return await db
        }
    }
}
