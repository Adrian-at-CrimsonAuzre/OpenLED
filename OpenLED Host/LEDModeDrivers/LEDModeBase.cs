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
			Arduino_Host.WriteColor.RGB(Color.ToColor());
		}

		public LEDModeBase()
		{
			Properties.Settings.Default.PropertyChanged += SettingsChanged;
		}

		public virtual void SettingsChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) { }
	}
}
