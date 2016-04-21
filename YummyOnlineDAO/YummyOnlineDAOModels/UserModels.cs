using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YummyOnlineDAO.Models {
	public enum Role {
		/// <summary>
		/// 匿名
		/// </summary>
		Nemo = 0,
		/// <summary>
		/// 普通顾客
		/// </summary>
		Customer = 1,
		/// <summary>
		/// 超级管理员
		/// </summary>
		SuperAdmin = 2
	}
	public class User {
		[Key, MaxLength(10)]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public string Id { get; set; }

		[MaxLength(11)]
		public string PhoneNumber { get; set; }
		[MaxLength(128)]
		public string Email { get; set; }
		[MaxLength(20)]
		public string UserName { get; set; }
		public string PasswordHash { get; set; }

		public bool Confirmed { get; set; }
		public bool IsSendRecommedation { get; set; }

		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public DateTime CreateDate { get; set; }

		public ICollection<UserRole> Roles { get; set; }
	}
	public class UserRole {
		[Key, Column(Order = 0)]
		public string UserId { get; set; }
		public User User { get; set; }

		[Key, Column(Order = 1)]
		public Role Role { get; set; }
	}
}
