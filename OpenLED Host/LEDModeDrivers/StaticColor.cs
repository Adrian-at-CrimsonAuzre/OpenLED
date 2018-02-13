using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.ComponentModel;

namespace OpenLED_Host.LEDModeDrivers
{
    class StaticColor : LEDModeBase
    {
		public override void SettingsChanged(object sender, PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case (nameof(Properties.Settings.Default.LEDMode)):
				case (nameof(Properties.Settings.Default.ColorOne)):
					{
						if (Properties.Settings.Default.LEDMode == LEDModes.StaticColor)
							LEDModeBase.ColorOut(Properties.Settings.Default.ColorOne);
						else if (Properties.Settings.Default.LEDMode == LEDModes.Off)
							LEDModeBase.ColorOut(new HSLColor(0, 0.0, 0));
						break;
					}
			}
		}
	}
}
