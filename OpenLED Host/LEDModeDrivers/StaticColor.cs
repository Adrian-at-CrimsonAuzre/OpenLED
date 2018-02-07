using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace OpenLED_Host.LEDModeDrivers
{
    class StaticColor : LEDModeBase
    {
		private HSLColor _Color;
		/// <summary>
		/// The Color that is supposed to be set
		/// </summary>
		public HSLColor Color
		{
			get { return _Color; }
			set
			{
				_Color = value;
				ColorOut(Color);
				NotifyPropertyChanged();
			}
		}

	}
}
