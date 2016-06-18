using HotelDAO.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace HotelDAO {
	public partial class HotelManager {
		public async Task<List<StaffRoleSchema>> GetStaffRoles(string staffId) {
			ICollection<StaffRole> roles = await ctx.Staffs
				.Include(p => p.StaffRoles)
				.Where(p => p.Id == staffId)
				.Select(p => p.StaffRoles)
				.FirstOrDefaultAsync();
			List<StaffRoleSchema> schemas = new List<StaffRoleSchema>();
			if(roles == null) {
				return schemas;
			}
			foreach(StaffRole role in roles) {
				List<StaffRoleSchema> tSchemas = await ctx.StaffRoleSchemas.Where(p => p.StaffRole.Id == role.Id).ToListAsync();
				tSchemas?.ForEach(s => {
					if(!schemas.Contains(s)) {
						schemas.Add(s);
					}
				});
			}
			return schemas;
		}
		public async Task<bool> IsStaffHasSchema(string staffId, Schema schema) {
			List<StaffRoleSchema> schemas = await GetStaffRoles(staffId);
			if(schemas.Count == 0) {
				return false;
			}
			foreach(StaffRoleSchema s in schemas) {
				if(s.Schema == schema) {
					return true;
				}
			};
			return false;
		}
		public async Task<dynamic> GetStaffs() {
			return await ctx.Staffs.Select(p => new {
				p.Id,
				p.Name,
				Schemas = p.StaffRoles.SelectMany(pp => pp.Schemas.Select(s => s.Schema))
			}).ToListAsync();
		}
	}
}