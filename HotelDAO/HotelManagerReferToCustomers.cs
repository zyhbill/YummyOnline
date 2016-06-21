using HotelDAO.Models;
using System.Data.Entity;
using System.Threading.Tasks;

namespace HotelDAO {
	public partial class BaseHotelManager {
		public async Task<Customer> GetOrCreateCustomer(string userId) {
			Customer customer = await ctx.Customers.FirstOrDefaultAsync(p => p.Id == userId);
			if(customer == null) {
				customer = new Customer {
					Id = userId,
					Points = 0,
				};
				ctx.Customers.Add(customer);
				await ctx.SaveChangesAsync();
			}
			return customer;
		}
	}
}