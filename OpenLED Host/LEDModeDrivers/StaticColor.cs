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
						if(Properties.Settings.Default.LEDMode == LEDModes.SingleColor)
							LEDModeBase.ColorOut(Properties.Settings.Default.ColorOne);
						break;
					}
			}
		}
	}
}
