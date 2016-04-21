namespace OrderSystem.Models {
	public class SignupViewModel {
		public string PhoneNumber { get; set; }
		public string Code { get; set; }
		public string Password { get; set; }
		public string PasswordAga { get; set; }
	}

	public class SigninViewModel {
		public string PhoneNumber { get; set; }
		public string Password { get; set; }
		public string CodeImg { get; set; }
		public bool RememberMe { get; set; }
	}

	public class ForgetViewModel {
		public string PhoneNumber { get; set; }
		public string Code { get; set; }
		public string Password { get; set; }
		public string PasswordAga { get; set; }
	}
}