using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenLED_Host.LEDModeDrivers
{
	public class LEDModeBase : Models.NotifyBase
	{
		public static void ColorOut(HSLColor Color)
		{
			Arduino_Host.WriteColor.SingleRGB((byte)Properties.Settings.Default.LEDMode, Color.ToColor());
		}
		public static void DualColorOut(HSLColor ColorOne, HSLColor ColorTwo, byte EffectSpeed)
		{
			Arduino_Host.WriteColor.DoubleRGB((byte)Properties.Settings.Default.LEDMode, ColorOne, ColorTwo, EffectSpeed);
		}
		public LEDModeBase()
		{
			Properties.Settings.Default.PropertyChanged += SettingsChanged;
		}

		private void SettingsChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch(Properties.Settings.Default.LEDMode)
			{
				case (LEDModes.Off):
					{
						ColorOut(new HSLColor(0, 0, 0.0));
						break;
					}
				case (LEDModes.StaticColor):
					{
						ColorOut(Properties.Settings.Default.ColorOne);
						break;
					}
				case (LEDModes.Breathing):
					{
						DualColorOut(Properties.Settings.Default.ColorOne, Properties.Settings.Default.ColorTwo, Properties.Settings.Default.EffectSpeed);
						break;
					}
				case (LEDModes.HeartBeat):
					{
						DualColorOut(Properties.Settings.Default.ColorOne, Properties.Settings.Default.ColorTwo, Properties.Settings.Default.EffectSpeed);
						break;
					}
				case (LEDModes.Strobe):
					{
						DualColorOut(Properties.Settings.Default.ColorOne, Properties.Settings.Default.ColorTwo, Properties.Settings.Default.EffectSpeed);
						break;
					}
				case (LEDModes.Cycle):
					{
						DualColorOut(Properties.Settings.Default.ColorOne, Properties.Settings.Default.ColorTwo, Properties.Settings.Default.EffectSpeed);
						break;
					}
				case (LEDModes.Rainbow):
					{
						DualColorOut(Properties.Settings.Default.ColorOne, Properties.Settings.Default.ColorTwo, Properties.Settings.Default.EffectSpeed);
						break;
					}
				case (LEDModes.ColorReactive):
					{
						break;
					}
				case (LEDModes.Visualizer):
					{
						break;
					}
			}
		}
	}
}