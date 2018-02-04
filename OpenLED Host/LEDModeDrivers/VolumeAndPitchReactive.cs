using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenLED_Host.LEDModeDrivers
{
	public static class VolumeAndPitchReactive
	{
		public static HSLColor GetAVGHSLColor(List<double> Data)
		{
			List<HSLColor> Colors = GetHSLColors(Data);
			List<int> Peaks = Sound_Library.BassEngine.PeakDetection(Data);

			List<HSLColor> TopHSLValues = new List<HSLColor>();
			for (int i = 0; i < Peaks.Count; i++)
				if (Peaks[i] > 0)
					TopHSLValues.Add(Colors[i]);

			double AVGH = 0;
			for (int i = 0; i < TopHSLValues.Count(); i++)
				if (TopHSLValues[i].Luminosity > 0)
					AVGH = (AVGH * i + TopHSLValues[i].Hue) / (i + 1);

			//Get a good Value for the background color that is related to how "loud" the sound is by selecting the top 10%
			HSLColor Average = new HSLColor(AVGH, 1, TopHSLValues.Count > 0 ? TopHSLValues.OrderByDescending(x => x.Luminosity).Take((int)(Math.Round(TopHSLValues.Count * .1) > 0 ? Math.Round(TopHSLValues.Count * .1) : TopHSLValues.Count)).Average(x => x.Luminosity) : 0);
			if (double.IsNaN(Average.Luminosity))
				Average.Luminosity = 0;

			//Leave a breakpoint here. This function once reported an L of 38 and an H of 6 which should be impossible.
			if (Average.Luminosity > 1 || Average.Hue > 1)
				Average = Average;

			return Average;
		}

		/// <summary>
		/// Gets HSL colors for a list of values
		/// </summary>
		/// <param name="Data">List of values ranging from 0 to 1, all data out of this range will be give HSLColor(0,0,0)</param>
		/// <returns>List of HSLColors</returns>
		public static List<HSLColor> GetHSLColors(List<double> Data)
		{
			List<HSLColor> Colors = new List<HSLColor>();

			for (int i = 0; i < Data.Count; i++)
				if (Data[i] <= 1 && Data[i] >= 0)
					Colors.Add(new HSLColor((double)i / Data.Count, 1, Data[i]));
				else
					Colors.Add(new HSLColor(0,0,0));

			return Colors;
		}
	}
}
