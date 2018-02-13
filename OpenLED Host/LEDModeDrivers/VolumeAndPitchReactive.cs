using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows.Threading;

namespace OpenLED_Host.LEDModeDrivers
{
	/// <summary>
	/// Full of all of the functions that make this possible to standardize throughout the application
	/// </summary>
	public class VolumeAndPitchReactive : LEDModeBase
	{
		#region Fields
		private int _BlendedFrames = 10;
		/// <summary>
		/// Number of color frames that will be blended
		/// </summary>
		public int BlendedFrames
		{
			get { return _BlendedFrames; }
			set
			{
				_BlendedFrames = value;
				NotifyPropertyChanged();
			}
		}

		public enum ColorCalculationModes
		{
			/// <summary>
			/// Linear Calculation using raw averages.
			/// </summary>
			Linear,
			/// <summary>
			/// Uses polar coordinates to find a more propper color mix
			/// </summary>
			Angular,
			/// <summary>
			/// Linear Calculation using raw averages BUT values are scaled through the magic of vectors
			/// </summary>
			VectorLinear,
			/// <summary>
			/// Uses polar coordinates to find a more propper color mix AND uses volume to determine mixing %
			/// </summary>
			VectorAngular
		}

		private ColorCalculationModes _ColorCalculationMode = ColorCalculationModes.VectorAngular;
		/// <summary>
		/// Mode color should be calculated in
		/// </summary>
		public ColorCalculationModes ColorCalculationMode
		{
			get { return _ColorCalculationMode; }
			set
			{
				_ColorCalculationMode = value;
				NotifyPropertyChanged();
			}
		}

		public enum BrightnessCalculationModes
		{
			/// <summary>
			/// Averages all brightnesses, good for slow music
			/// </summary>
			Average,
			/// <summary>
			/// Averages top 50% of all brightnesses
			/// </summary>
			Top50,
			/// <summary>
			/// Averages top 25% of all brightnesses
			/// </summary>
			Top25,
			/// <summary>
			/// Averages top 10% of all brightnesses
			/// </summary>
			Top10,
			/// <summary>
			/// Takes BlendedFrames/2 LATEST frames, much more reactive than any of the averaging settings
			/// </summary>
			BlendedOver2
		}

		private BrightnessCalculationModes _BrightnessCalculationMode = BrightnessCalculationModes.Top25;
		/// <summary>
		/// Mode Brightness should be calculated in
		/// </summary>
		public BrightnessCalculationModes BrightnessCalculationMode
		{
			get { return _BrightnessCalculationMode; }
			set
			{
				_BrightnessCalculationMode = value;
				NotifyPropertyChanged();
			}
		}


		private List<HSLColor> _ColorsToBlend = new List<HSLColor>();
		/// <summary>
		/// Colors to be used for blending
		/// </summary>
		/// <seealso cref="BlendedFrames"/>
		public List<HSLColor> ColorsToBlend
		{
			get { return _ColorsToBlend; }
			set
			{
				_ColorsToBlend = value;
				NotifyPropertyChanged();
			}
		}

		private int _MaximumFrequency = 2000;
		/// <summary>
		/// Max allowed frequency, defaults to 2000
		/// </summary>
		public int MaximumFrequency
		{
			get { return _MaximumFrequency; }
			set
			{
				_MaximumFrequency = value;
				NotifyPropertyChanged();
			}
		}

		private int _MinimumFrequency = 0;
		/// <summary>
		/// Min allowed frequency, defaults to 0
		/// </summary>
		public int MinimumFrequency
		{
			get { return _MinimumFrequency; }
			set
			{
				_MinimumFrequency = value;
				NotifyPropertyChanged();
			}
		}

		private List<(HSLColor hsl, int p)> _ColorsAndPeaks = new List<(HSLColor hsl, int p)>();
		/// <summary>
		/// Contains data for the Colors and Peaks of FFT data
		/// </summary>
		public List<(HSLColor hsl, int p)> ColorsAndPeaks
		{
			get { return _ColorsAndPeaks; }
			set
			{
				_ColorsAndPeaks = value;
				NotifyPropertyChanged();
			}
		}

		private HSLColor _AverageColor = new HSLColor(0, 0, 0);
		/// <summary>
		/// Average Color of of FFT data
		/// </summary>
		public HSLColor AverageColor
		{
			get { return _AverageColor; }
			set
			{
				_AverageColor = value;
				NotifyPropertyChanged();
			}
		}

		#endregion Fields
		private Timer ReactiveTimer = new Timer()
		{
			Interval = 20
		};

		public VolumeAndPitchReactive()
		{
			ReactiveTimer.Elapsed += ReactiveTimer_Tick;
		}		

		public void StartReacting()
		{
			ReactiveTimer.Start();
		}

		public void StopReacting()
		{
			ReactiveTimer.Stop();
		}
		private bool ticking = false;
		private void ReactiveTimer_Tick(object sender, EventArgs e)
		{
			if (!ticking)
			{
				ticking = true;
				GenerateColorData();
				ticking = false;
			}
			else { }
		}

		public void GenerateColorData()
		{
			float[] channelData = new float[(int)Sound_Library.BassEngine.Instance.FFT];
			Sound_Library.BassEngine.Instance.GetFFTData(channelData);

			List<(double v, int p)> VolumesAndPeaks = new List<(double, int)>();

			int scaleFactorLinear = 5;
			bool allzeros = true;

			int maximumFrequencyIndex = Sound_Library.BassEngine.Instance.GetFFTFrequencyIndex(MaximumFrequency);
			int minimumFrequencyIndex = Sound_Library.BassEngine.Instance.GetFFTFrequencyIndex(MinimumFrequency);

			//only reason we have this is so we can update the ColorsAndPeaks in one swoop, instead of piecemeal
			List<(HSLColor hsl, int peak)> TempHSLandPeak = new List<(HSLColor hsl, int peak)>();

			for (int i = minimumFrequencyIndex; i <= maximumFrequencyIndex; i++)
			{
				double fftBucketHeight = (channelData[i] * scaleFactorLinear);

				if (fftBucketHeight > 1e-4)
					allzeros = false;


				//Keep peaks in range incase weird shit has happened
				if (fftBucketHeight < 0f)
					fftBucketHeight = 0f;
				if (fftBucketHeight > 1)
					fftBucketHeight = 1;

				//This is used later for calculating colors and positions
				VolumesAndPeaks.Add((fftBucketHeight, 0));
			}
			List<HSLColor> TopHSLValues = new List<HSLColor>();
			//If all the data was zeros, then don't update
			if (!allzeros)
			{
				List<int> Peaks = Sound_Library.BassEngine.PeakDetection(VolumesAndPeaks.Select(x => x.v).ToList(), .01);
				for (int i = 0; i < Peaks.Count; i++)
				{
					if (Peaks[i] > 0)
						TopHSLValues.Add(new HSLColor((double)i / VolumesAndPeaks.Count, 1, VolumesAndPeaks[i].v));
					VolumesAndPeaks[i] = (VolumesAndPeaks[i].v, Peaks[i]);

					TempHSLandPeak.Add((new HSLColor(new HSLColor((double)i / VolumesAndPeaks.Count, 1, VolumesAndPeaks[i].v)), VolumesAndPeaks[i].p));
				}
			}
			else
			{
				//Zero out TopHSL
				TopHSLValues.Add(new HSLColor(0, 0, 0));
				TempHSLandPeak = Enumerable.Repeat<(HSLColor, int)>((new HSLColor(0, 0, 0), 0), maximumFrequencyIndex - minimumFrequencyIndex).ToList();
			}
			//add current color average to end of list, and remove extras if needed
			ColorsToBlend.Add(GetAVGHSLColor(TopHSLValues, ColorCalculationMode, BrightnessCalculationMode));
			if (ColorsToBlend.Count() > BlendedFrames + 1)
				for (int i = ColorsToBlend.Count(); i > BlendedFrames + 1; i--)
					ColorsToBlend.RemoveRange(0, 1);

			//average last [BlendedFrames] background colors together
			AverageColor = GetAVGHSLColor(ColorsToBlend, ColorCalculationMode, BrightnessCalculationModes.BlendedOver2);
						

			//Write color to all areas
			ColorOut(AverageColor);
			//print the color we're using
			Console.WriteLine(AverageColor);
			//update out list of ColorsAndPeaks, which a control depends on
			ColorsAndPeaks = TempHSLandPeak;
		}

		/// <summary>
		/// Gets the Average HSL color from a list
		/// </summary>
		/// <param name="Colors">List of HSL colors to average</param>
		/// <returns>An average of all the HSL colors given</returns>
		private HSLColor GetAVGHSLColor(List<HSLColor> Colors, ColorCalculationModes color, BrightnessCalculationModes brightness)
		{
			HSLColor ret = new HSLColor(0.0,1,0);
			List<HSLColor> TempColors = new List<HSLColor>(Colors);
			if (TempColors.Count > 0)
			{

				switch (color)
				{
					case (ColorCalculationModes.Angular):
						{
							double s = TempColors.Average(x => x.Saturation);
							double l = TempColors.Average(x => x.Luminosity);
							double h = 0;

							double a1 = 0;
							double a2 = 0;
							for (int i = 0; i < TempColors.Count; i++)
							{
								a1 += Math.Sin(2 * Math.PI * TempColors[i].Hue);
								a2 += Math.Cos(2 * Math.PI * TempColors[i].Hue);
							}
							if (a1 != 0 || a2 != 0)
								h = Math.Atan2(a1, a2) / (2 * Math.PI);

							if (h < 0)
								h++;
							ret = new HSLColor(h, s, l);
							break;
						}
					case (ColorCalculationModes.Linear):
						{
							double AVGH = 0;
							for (int i = 0; i < TempColors.Count(); i++)
								if (TempColors[i].Luminosity > 0)
									AVGH = (AVGH * i + TempColors[i].Hue) / (i + 1);

							//Get a good Value for the background color that is related to how "loud" the sound is by selecting the top 10%
							ret = new HSLColor(AVGH, 1, TempColors.Count > 0 ? TempColors.OrderByDescending(x => x.Luminosity).Take((int)(Math.Round(TempColors.Count * .1) > 0 ? Math.Round(TempColors.Count * .1) : TempColors.Count)).Average(x => x.Luminosity) : 0);
							if (double.IsNaN(ret.Luminosity))
								ret.Luminosity = 0;
							break;
						}
					case (ColorCalculationModes.VectorAngular):
						{
							List<double> x = new List<double>();
							List<double> y = new List<double>();
							foreach (var c in TempColors)
							{
								x.Add(c.Luminosity * Math.Cos(c.Hue * 2 * Math.PI));
								y.Add(c.Luminosity * Math.Sin(c.Hue * 2 * Math.PI));
							}

							double AverageX = x.Average();
							double AverageY = y.Average();

							double theta = Math.Atan(AverageY / AverageX);
							if (theta < 0 && theta > -Math.PI)
								theta = theta + Math.PI;
							if (AverageX < 0)
								theta = theta + Math.PI;

							if (double.IsNaN(theta))
								theta = 0;
							ret.Hue = theta / (2 * Math.PI);

							break;
						}
					case (ColorCalculationModes.VectorLinear):
						{
							//TODO: VectorLinear color calc
							break;
						}
				}
				
				switch(brightness)
				{
					case (BrightnessCalculationModes.Average):
						{
							ret.Luminosity = TempColors.Average(z => z.Luminosity);
							break;
						}
					case (BrightnessCalculationModes.BlendedOver2):
						{
							TempColors.Reverse();
							ret.Luminosity = TempColors.Take(BlendedFrames/2 > .5 ? 1: BlendedFrames/2).Average(z=>z.Luminosity);
							break;
						}
					case (BrightnessCalculationModes.Top10):
						{
							ret.Luminosity = TempColors.OrderByDescending(z => z.Luminosity).Take((int)Math.Round(TempColors.Count * .1 < .5 ? 1 : TempColors.Count * .25, MidpointRounding.AwayFromZero)).Average(z => z.Luminosity);
							break;
						}
					case (BrightnessCalculationModes.Top25):
						{
							ret.Luminosity = TempColors.OrderByDescending(z => z.Luminosity).Take((int)Math.Round(TempColors.Count * .25 < .5 ? 1 : TempColors.Count * .25, MidpointRounding.AwayFromZero)).Average(z => z.Luminosity);
							break;
						}
					case (BrightnessCalculationModes.Top50):
						{
							ret.Luminosity = TempColors.OrderByDescending(z => z.Luminosity).Take((int)Math.Round(TempColors.Count * .5 < .5 ? 1 : TempColors.Count * .25, MidpointRounding.AwayFromZero)).Average(z => z.Luminosity);
							break;
						}
				}
				
			}
			return ret;
		}

		/// <summary>
		/// Gets HSL colors for a list of values
		/// </summary>
		/// <param name="Data">List of values ranging from 0 to 1, all data out of this range will be give HSLColor(0,0,0)</param>
		/// <returns>List of HSLColors</returns>
		private static List<HSLColor> GetHSLColors(List<double> Data)
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
