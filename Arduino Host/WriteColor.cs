using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

//TODO: Better communication between program and Arduino
namespace Arduino_Host
{
	/// <summary>
	/// Write a color to an Arduino Light Controller in a variety of formats.
	/// </summary>
    public static class WriteColor
    {
		/// <summary>
		/// Sends an Arduino Light Controller RGB color data.
		/// </summary>
		/// <param name="color">RGB Color to send</param>
		/// <param name="COMPort">Option COM port to use, will autodetect otherwise</param>
		/// <returns></returns>
		public static bool RGB(System.Drawing.Color color, string COMPort = "")
		{
			try
			{
				//TODO: Find better method of doing this, wouldn't want ot try writing RGB data to a Serial CNC or something
				if (string.IsNullOrEmpty(COMPort) || !SerialPort.GetPortNames().Contains(COMPort))
					COMPort = SerialPort.GetPortNames().First();

				using (SerialPort serial = new SerialPort(COMPort, 115200))
				{
					serial.Open();
					serial.Write(new byte[] { color.R, color.G, color.B }, 0, 3);
				}

				return true;
			}
			catch
			{
				return false;
			}
		}
    }
}
