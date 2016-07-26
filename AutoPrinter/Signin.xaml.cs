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
using System.Diagnostics;

namespace AutoPrinter {
	/// <summary>
	/// Signin.xaml 的交互逻辑
	/// </summary>
	public partial class Signin : Window {
		public Signin() {
			InitializeComponent();

			Process[] tProcess = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
			if(tProcess.Length > 1) {
				MessageBox.Show("已经开启一个程序，请关闭后重试", "重复启动", MessageBoxButton.OK, MessageBoxImage.Error);
				Application.Current.Shutdown();
			}

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

		private void textBoxSigninName_KeyDown(object sender, KeyEventArgs e) {
			if(e.Key == Key.Enter) {
				buttonSignin.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent, buttonSignin));
			}
		}

		private void passwordBox_KeyDown(object sender, KeyEventArgs e) {
			if(e.Key == Key.Enter) {
				buttonSignin.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent, buttonSignin));
			}
		}
	}
}
