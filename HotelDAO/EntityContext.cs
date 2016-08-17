using System.Data.Entity;

namespace HotelDAO.Models {
	public class HotelContext : DbContext {
		public HotelContext(string connString)
			: base(connString) {
			Database.SetInitializer<HotelContext>(null);
		}
		protected override void OnModelCreating(DbModelBuilder modelBuilder) {
			modelBuilder.Entity<Dine>()
				.HasMany(e => e.Remarks)
				.WithMany(e => e.Dines)
				.Map(m => m.ToTable("DineRemarks").MapLeftKey("Dine_Id").MapRightKey("Remark_Id"));

			modelBuilder.Entity<DineMenu>()
				.HasMany(e => e.Remarks)
				.WithMany(e => e.DineMenus)
				.Map(m => m.ToTable("DineMenuRemarks").MapLeftKey("DineMenu_Id").MapRightKey("Remark_Id"));

			modelBuilder.Entity<Menu>()
				.HasMany(e => e.Remarks)
				.WithMany(e => e.Menus)
				.Map(m => m.ToTable("MenuRemarks").MapLeftKey("Menu_Id").MapRightKey("Remark_Id"));

			modelBuilder.Entity<MenuClass>()
				.HasMany(e => e.Menus)
				.WithMany(e => e.Classes)
				.Map(m => m.ToTable("MenuClassMenus").MapLeftKey("MenuClass_Id").MapRightKey("Menu_Id"));

			modelBuilder.Entity<StaffRole>()
				.HasMany(e => e.Staffs)
				.WithMany(e => e.StaffRoles)
				.Map(m => m.ToTable("StaffStaffRoles").MapLeftKey("StaffRole_Id").MapRightKey("Staff_Id"));
		}

		public DbSet<Department> Departments { get; set; }
		public DbSet<Printer> Printers { get; set; }
		public DbSet<PrinterFormat> PrinterFormats { get; set; }

		public DbSet<Area> Areas { get; set; }
		public DbSet<Desk> Desks { get; set; }

		public DbSet<MenuClass> MenuClasses { get; set; }
		public DbSet<Menu> Menus { get; set; }
		public DbSet<MenuPrice> MenuPrice { get; set; }
		public DbSet<MenuOnSale> MenuOnSales { get; set; }
		public DbSet<MenuSetMeal> MenuSetMeals { get; set; }

		public DbSet<Dine> Dines { get; set; }
		public DbSet<DineMenu> DineMenus { get; set; }
		public DbSet<DinePaidDetail> DinePaidDetails { get; set; }
		public DbSet<ReturnedReason> ReturnedReasons { get; set; }
		public DbSet<TakeOut> TakeOuts { get; set; }
		public DbSet<Invoice> Invoices { get; set; }

		public DbSet<PayKind> PayKinds { get; set; }
		public DbSet<TimeDiscount> TimeDiscounts { get; set; }
		public DbSet<VipDiscount> VipDiscounts { get; set; }

		public DbSet<Remark> Remarks { get; set; }

		public DbSet<Reserve> Reserve { get; set; }


		public DbSet<HotelConfig> HotelConfigs { get; set; }

		public DbSet<Customer> Customers { get; set; }
		public DbSet<VipLevel> VipLevels { get; set; }

		public DbSet<Staff> Staffs { get; set; }
		public DbSet<StaffRole> StaffRoles { get; set; }
		public DbSet<StaffRoleSchema> StaffRoleSchemas { get; set; }

		public DbSet<PayKindShift> PayKindShifts { get; set; }

		public DbSet<Log> Logs { get; set; }
	}
}