using Protocol.PrintingProtocol;
using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Net;
using System.Threading.Tasks;

namespace AutoPrinter {
	public class ShiftDriverPrinter : BaseShiftPrinter {
		public void Printer(ShiftForPrinting protocol) {
			PrintDocument pd = new PrintDocument();
			pd.PrinterSettings.PrinterName = protocol.Printer.Name;
			pd.PrintPage += (sender, e) => {
				drawShift(e.Graphics, protocol);
			};
			pd.Print();
		}
	}
	public class ShiftPrinter : BaseShiftPrinter {
		public async Task Print(ShiftForPrinting protocol) {
			handleIPPrinterFormat(protocol.PrinterFormat);

			IPAddress ip = IPAddress.Parse(protocol.Printer.IpAddress);
			Bitmap bmp = generateShiftBmp(protocol);
			await IPPrinter.GetInstance().Print(ip, bmp, protocol.PrinterFormat.ColorDepth);
		}
	}
}
