using HotelDAO.Models;
using System.Threading.Tasks;

namespace HotelDAO {
	public abstract class BaseHotelManager {
		public BaseHotelManager(string connString) {
			ctx = new HotelContext(connString);
		}
		protected HotelContext ctx;
	}
}
