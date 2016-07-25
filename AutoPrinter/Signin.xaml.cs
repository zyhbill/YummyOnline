using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Utility;
using Protocol;
using Newtonsoft.Json;

namespace AutoPrinter {
	/// <summary>
	/// Signin.xaml 的交互逻辑
	/// </summary>
	public partial class Signin : Window {
		public Signin() {
			InitializeComponent();

			try {
				textBlockVersion.Text = $" {ApplicationDeployment.CurrentDeployment.CurrentVersion}";
			}
			catch { }

#if DEBUG
			textBlockVersion.Text = $"DEBUG调试版本";
#elif COMPANYSERVER
			textBlockVersion.Text = $"COMPANYSERVER调试版本";
#endif
		}

		private async void buttonSignin_Click(object sender, RoutedEventArgs e) {
			buttonSignin.Content = "正在登录...";
			buttonSignin.IsEnabled = false;

			string jsonResult = await HttpPost.PostAsync(Config.RemoteSigninUrl, new {
				SigninName = textBoxSigninName.Text,
				Password = passwordBox.Password
			});

			buttonSignin.Content = "登录";
			buttonSignin.IsEnabled = true;

			if(jsonResult == null) {
				MessageBox.Show("连接登陆服务器失败", "失败", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			JsonError result = JsonConvert.DeserializeObject<JsonError>(jsonResult);
			if(!result.Succeeded) {
				MessageBox.Show(result.ErrorMessage, "失败", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			MainWindow mainWindow = new MainWindow();
			mainWindow.Show();
			Close();
		}
	}
}
