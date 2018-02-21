using System;
using System.Linq;
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
		/// <param name="Color">HSL Color. [0] = H, [1] = S, [2] = L</param>
		/// <param name="COMPort">Optional COM port to use, will autodetect otherwise</param>
		public static void SingleRGB(byte ArduinoMode, double[] Color, string COMPort = "")
		{
			DoubleRGB(ArduinoMode, Color, new double[] { 0, 0, 0 }, 0, COMPort);
		}

		/// <summary>
		/// Sends an Arduino Light Controller RGB color data.
		/// </summary>
		/// <param name="ArduinoMode">ArduinoMode being selected. See Arduino Client>Types.h or OpenLED Host>LEDModes</param>
		/// <param name="ColorOne">HSL Color. [0] = H, [1] = S, [2] = L</param>
		/// <param name="ColorTwo">HSL Color. [0] = H, [1] = S, [2] = L</param>
		/// <param name="COMPort">Optional COM port to use, will autodetect otherwise</param>
		public static void DoubleRGB(byte ArduinoMode, double[] ColorOne, double[] ColorTwo, byte EffectSpeed, string COMPort = "" )
		{
			if(!Lock)
			{
				Lock = true;
				ThreadPool.QueueUserWorkItem(delegate
				{
					ColorOne = HSLToHSV(ColorOne);
					ColorTwo = HSLToHSV(ColorTwo);

					try
					{
						//TODO: Find better method of doing this, wouldn't want to try writing HSV data to a Serial CNC or something
						if (string.IsNullOrEmpty(COMPort) || !SerialPort.GetPortNames().Contains(COMPort))
							COMPort = SerialPort.GetPortNames().First();

						using (SerialPort serial = new SerialPort(COMPort, BaudRate))
						{
							serial.Open();
							serial.Write(new byte[] { ArduinoMode, (byte)(ColorOne[0]*255), (byte)(ColorOne[1] * 255), (byte)(ColorOne[2] * 255), (byte)(ColorTwo[0] * 255), (byte)(ColorTwo[1] * 255), (byte)(ColorTwo[2] * 255), EffectSpeed }, 0, 8);
						}
					}
					catch
					{
					}
					Lock = false;
				});
			}
		}

		/// <summary>
		/// Converts HSL color to HSB, which FastLED Accepts
		/// </summary>
		/// <param name="Color">HSL Color. [0] = H, [1] = S, [2] = L</param>
		/// <returns></returns>
		private static double[] HSLToHSV(double[] Color)
		{
			double ColorS = Color[1];
			double ColorL = Color[2];
			double temp = 0;
			double OutS = 0;
			double OutB = 0;

			temp = ColorS * (ColorL < .5 ? ColorL : 1 - ColorL);
			OutB = ColorL + temp;
			OutS = ColorL > 0 ? 2 * temp / OutB : 0;

			return new double[] { Color[0], OutS, OutB };
		}
    }
}
