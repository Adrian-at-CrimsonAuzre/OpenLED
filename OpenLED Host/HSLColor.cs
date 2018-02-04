using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace OpenLED_Host
{
	/// <summary>
	/// Class that stores and deals with HSL Colors
	/// </summary>
	public class HSLColor
	{
		// Private data members below are on scale 0-1
		// They are scaled for use externally based on scale
		private double hue = 1.0;
		private double saturation = 1.0;
		private double luminosity = 1.0;

		private const double scale = 240.0;

		public double Hue
		{
			get { return hue * scale; }
			set { hue = CheckRange(value / scale); }
		}
		public double Saturation
		{
			get { return saturation * scale; }
			set { saturation = CheckRange(value / scale); }
		}
		public double Luminosity
		{
			get { return luminosity * scale; }
			set { luminosity = CheckRange(value / scale); }
		}

		private double CheckRange(double value)
		{
			if (value < 0.0)
				value = 0.0;
			else if (value > 1.0)
				value = 1.0;
			return value;
		}

		public override string ToString()
		{
			return String.Format("H: {0:#0.##} S: {1:#0.##} L: {2:#0.##}", Hue, Saturation, Luminosity);
		}

		public string ToRGBString()
		{
			Color color = (Color)this;
			return String.Format("R: {0:#0.##} G: {1:#0.##} B: {2:#0.##}", color.R, color.G, color.B);
		}

		public Color ToColor()
		{
			return this;
		}

		#region Casts to/from System.Drawing.Color
		public static implicit operator Color(HSLColor hslColor)
		{
			double r = 0, g = 0, b = 0;
			if (hslColor.Luminosity != 0)
			{
				if (hslColor.saturation == 0)
					r = g = b = hslColor.Luminosity;
				else
				{
					double temp2 = GetTemp2(hslColor);
					double temp1 = 2.0 * hslColor.Luminosity - temp2;

					r = GetColorComponent(temp1, temp2, hslColor.Hue + 1.0 / 3.0);
					g = GetColorComponent(temp1, temp2, hslColor.Hue);
					b = GetColorComponent(temp1, temp2, hslColor.Hue - 1.0 / 3.0);
				}
			}
			return Color.FromArgb((int)(255 * r), (int)(255 * g), (int)(255 * b));
		}

		private static double GetColorComponent(double temp1, double temp2, double temp3)
		{
			temp3 = MoveIntoRange(temp3);
			if (temp3 < 1.0 / 6.0)
				return temp1 + (temp2 - temp1) * 6.0 * temp3;
			else if (temp3 < 0.5)
				return temp2;
			else if (temp3 < 2.0 / 3.0)
				return temp1 + ((temp2 - temp1) * ((2.0 / 3.0) - temp3) * 6.0);
			else
				return temp1;
		}
		private static double MoveIntoRange(double temp3)
		{
			if (temp3 < 0.0)
				temp3 += 1.0;
			else if (temp3 > 1.0)
				temp3 -= 1.0;
			return temp3;
		}
		private static double GetTemp2(HSLColor hslColor)
		{
			double temp2;
			if (hslColor.Luminosity < 0.5)  //<=??
				temp2 = hslColor.Luminosity * (1.0 + hslColor.Saturation);
			else
				temp2 = hslColor.Luminosity + hslColor.Saturation - (hslColor.Luminosity * hslColor.Saturation);
			return temp2;
		}

		public static implicit operator HSLColor(Color color)
		{
			HSLColor hslColor = new HSLColor();
			hslColor.Hue = color.GetHue() / 360.0; // we store hue as 0-1 as opposed to 0-360 
			hslColor.Luminosity = color.GetBrightness();
			hslColor.Saturation = color.GetSaturation();
			return hslColor;
		}
		#endregion

		#region Operators
		public static HSLColor operator +(HSLColor c1, HSLColor c2)
		{
			return new HSLColor((c1.Hue + c2.Hue) / 2, (c1.Saturation + c2.Saturation) / 2, (c1.Luminosity + c2.Luminosity) / 2);
		}

		#endregion Operators
		public void SetRGB(int red, int green, int blue)
		{
			HSLColor hslColor = (HSLColor)Color.FromArgb(red, green, blue);
			this.Hue = hslColor.Hue;
			this.Saturation = hslColor.Saturation;
			this.Luminosity = hslColor.Luminosity;
		}

		public HSLColor() { }
		public HSLColor(Color color)
		{
			SetRGB(color.R, color.G, color.B);
		}
		public HSLColor(int red, int green, int blue)
		{
			SetRGB(red, green, blue);
		}
		public HSLColor(double hue, double saturation, double luminosity)
		{
			this.Hue = hue;
			this.Saturation = saturation;
			this.Luminosity = luminosity;
		}

	}
}
