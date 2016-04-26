using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using YummyOnlineDAO.Models;

namespace YummyOnlineDAO.Identity {
	public class StaffManager : BaseUserManager {
		public async Task<Staff> FindStaffById(string staffId) {
			return await ctx.Staffs.FirstOrDefaultAsync(p => p.Id == staffId);
		}
		public async Task<Staff> FindStaffBySigninName(string signinName) {
			return await ctx.Staffs.FirstOrDefaultAsync(p => p.SigninName == signinName);
		}
		public async Task<bool> CheckPasswordAsync(Staff user, string password) {
			Staff realUser = await ctx.Staffs.FirstOrDefaultAsync(p => p.Id == user.Id);
			if(realUser.PasswordHash != GetMd5(password)) {
				return false;
			}
			return true;
		}
	}
}
