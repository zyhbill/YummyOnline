using System;
using System.Net;
using System.Windows.Data;
using System.Windows.Media;

namespace AutoPrinter {
	public enum LogLevel {
		Info = 0,
		Success = 1,
		Warning = 2,
		Error = 3,
	}
	public class BaseLog {
		public string Message { get; set; }
		public LogLevel Level { get; set; }
	}
	public class ServerLog : BaseLog {
		public DateTime DateTime { get; set; }
	}
	public class IPPrinterLog : BaseLog {
		public DateTime DateTime { get; set; }
		public IPAddress IP { get; set; }
		public int? HashCode { get; set; }
	}
	public class IPPrinterStatus : BaseLog {
		public IPAddress IP { get; set; }
		public int WaitedCount { get; set; }
		public int IdleTime { get; set; }
	}

	public class LogFGConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			BaseLog p = (BaseLog)value;
			if(p.Level == LogLevel.Info) {
				return new SolidColorBrush(Colors.DarkBlue);
			}
			else if(p.Level == LogLevel.Success) {
				return new SolidColorBrush(Colors.DarkGreen);
			}
			else if(p.Level == LogLevel.Warning) {
				return new SolidColorBrush(Colors.Goldenrod);
			}
			else
				return new SolidColorBrush(Colors.Red);
		}
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
