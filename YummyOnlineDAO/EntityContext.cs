using System;
using System.Configuration;
using System.Data.Entity;

namespace YummyOnlineDAO.Models {
	public class YummyOnlineContext : DbContext {
		public YummyOnlineContext()
			: base(ConfigurationManager.ConnectionStrings["YummyOnlineConnString"].ConnectionString) {
			Database.SetInitializer<YummyOnlineContext>(null);
		}

		public DbSet<SystemConfig> SystemConfigs { get; set; }
		public DbSet<NewDineInformClientGuid> NewDineInformClientGuids { get; set; }
		public DbSet<Hotel> Hotels { get; set; }

		public DbSet<Log> Logs { get; set; }

		public DbSet<User> Users { get; set; }
		public DbSet<UserRole> UserRoles { get; set; }
		public DbSet<Staff> Staffs { get; set; }

		public DbSet<MenuGather> MenuGathers { get; set; }
		public DbSet<Nutrition> Nutritions { get; set; }
		public DbSet<Flavor> Flavors { get; set; }
	}
}
