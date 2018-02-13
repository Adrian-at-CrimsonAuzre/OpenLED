using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;

//TODO: Better communication between program and Arduino
namespace Arduino_Host
{
	/// <summary>
	/// Write a color to an Arduino Light Controller in a variety of formats.
	/// </summary>
    public static class WriteColor
    {
		/// <summary>
		/// Baud rate of arduino communication
		/// </summary>
		public static int BaudRate = 2000000;
		/// <summary>
		/// Sends an Arduino Light Controller RGB color data.
		/// </summary>
		/// <param name="ArduinoMode">ArduinoMode being selected. See Arduino Client>Types.h or OpenLED Host>LEDModes</param>
		/// <param name="Color">RGB Color to send</param>
		/// <param name="COMPort">Optional COM port to use, will autodetect otherwise</param>
		/// <returns></returns>
		public static void SingleRGB(byte ArduinoMode, System.Drawing.Color Color, string COMPort = "")
		{
			ThreadPool.QueueUserWorkItem(delegate
			{
				try
				{
					//TODO: Find better method of doing this, wouldn't want ot try writing RGB data to a Serial CNC or something
					if (string.IsNullOrEmpty(COMPort) || !SerialPort.GetPortNames().Contains(COMPort))
						COMPort = SerialPort.GetPortNames().First();

					using (SerialPort serial = new SerialPort(COMPort, BaudRate))
					{
						serial.Open();

						serial.Write(new byte[] { ArduinoMode, Color.R, Color.G, Color.B }, 0, 4);
					}
				}
				catch
				{
				}
			});
		}

		public static void DoubleRGB(byte ArduinoMode, System.Drawing.Color ColorOne, System.Drawing.Color ColorTwo, ushort FadeBetweenInMilliseconds, byte FadeMode, string COMPort = "" )
		{
			ThreadPool.QueueUserWorkItem(delegate
			{
				try
				{
					//TODO: Find better method of doing this, wouldn't want ot try writing RGB data to a Serial CNC or something
					if (string.IsNullOrEmpty(COMPort) || !SerialPort.GetPortNames().Contains(COMPort))
						COMPort = SerialPort.GetPortNames().First();

					using (SerialPort serial = new SerialPort(COMPort, BaudRate))
					{
						serial.Open();
						byte[] ShortBytes = BitConverter.GetBytes(FadeBetweenInMilliseconds);
						serial.Write(new byte[] { ArduinoMode, ColorOne.R, ColorOne.G, ColorOne.B, ColorTwo.R, ColorTwo.G, ColorTwo.B, ShortBytes[0], ShortBytes[1] }, 0, 4);
					}
				}
				catch
				{
				}
			});
		}
    }
}
