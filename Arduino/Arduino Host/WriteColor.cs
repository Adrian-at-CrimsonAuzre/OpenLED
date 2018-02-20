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
		/// Don't write if we're already writing
		/// </summary>
		private static bool Lock = false;

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
			DoubleRGB(ArduinoMode, Color, System.Drawing.Color.Black, 0, COMPort);
		}

		public static void DoubleRGB(byte ArduinoMode, System.Drawing.Color ColorOne, System.Drawing.Color ColorTwo, byte EffectSpeed, string COMPort = "" )
		{
			if(!Lock)
			{
				Lock = true;
				ThreadPool.QueueUserWorkItem(delegate
				{
					try
					{
						//TODO: Find better method of doing this, wouldn't want to try writing RGB data to a Serial CNC or something
						if (string.IsNullOrEmpty(COMPort) || !SerialPort.GetPortNames().Contains(COMPort))
							COMPort = SerialPort.GetPortNames().First();

						using (SerialPort serial = new SerialPort(COMPort, BaudRate))
						{
							serial.Open();
							serial.Write(new byte[] { ArduinoMode, ColorOne.R, ColorOne.G, ColorOne.B, ColorTwo.R, ColorTwo.G, ColorTwo.B, EffectSpeed }, 0, 8);
						}
					}
					catch
					{
					}
					Lock = false;
				});
			}
		}
    }
}
