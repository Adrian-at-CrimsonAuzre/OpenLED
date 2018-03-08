using System;
using System.Collections.Generic;
using System.ComponentModel;
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
		public enum ColorCalculationModes
		{
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
			Top10
		}

		private BrightnessCalculationModes _BrightnessCalculationMode = BrightnessCalculationModes.Top10;
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


		private int _MaximumFrequency = 5000;
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

		private List<HSLColor> _ColorData = new List<HSLColor>();
		/// <summary>
		/// Contains data for the Colors and Peaks of FFT data
		/// </summary>
		public List<HSLColor> ColorData
		{
			get { return _ColorData; }
			set
			{
				_ColorData = value;
				NotifyPropertyChanged();
			}
		}

		private List<bool> _Peaks = new List<bool>();
		/// <summary>
		/// Peaks Detected
		/// </summary>
		public List<bool> Peaks
		{
			get { return _Peaks; }
			set
			{
				_Peaks = value;
				NotifyPropertyChanged();
			}
		}

		private bool _PeakDetectionEnabled = false;
		/// <summary>
		/// Is peak detection enabled?
		/// </summary>
		public bool PeakDetectionEnabled
		{
			get { return _PeakDetectionEnabled; }
			set
			{
				_PeakDetectionEnabled = value;
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

		private int _ScaleFactor = 16;
		/// <summary>
		/// Changes the scale (and "volume") of the bars on the visualizer
		/// </summary>
		public int ScaleFactor
		{
			get { return _ScaleFactor; }
			set
			{
				_ScaleFactor = value;
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
			ReactiveTimer.Start();
			Properties.Settings.Default.PropertyChanged += SettingsChanged;
		}
		private void SettingsChanged(object sender, PropertyChangedEventArgs e)
		{
			//throw new NotImplementedException();
		}

		private bool ticking = false;
		private void ReactiveTimer_Tick(object sender, EventArgs e)
		{
			//Check to see if this mode is enabled, otherwise don't run
			if (Properties.Settings.Default.LEDMode == LEDModes.ColorReactive && !ticking)
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

			//List<(double v, int p)> VolumesAndPeaks = new List<(double, int)>();
			List<HSLColor> Colors = new List<HSLColor>();


			int maximumFrequencyIndex = Sound_Library.BassEngine.Instance.GetFFTFrequencyIndex(MaximumFrequency);
			int minimumFrequencyIndex = Sound_Library.BassEngine.Instance.GetFFTFrequencyIndex(MinimumFrequency);

			//only reason we have this is so we can update the ColorsAndPeaks in one swoop, instead of piecemeal

			for (int i = minimumFrequencyIndex; i <= maximumFrequencyIndex; i++)
			{
				double fftBucketHeight = channelData[i] * ScaleFactor;

				//Keep peaks above 0, capping the top of the peaks is done later to allow for a curve to be applied
				if (fftBucketHeight < 0f)
					fftBucketHeight = 0f;

				//Wolfram .5/(1+2^-(x*10-5))+1 from 0 to 1
				double VolumeCorrection = 1 / (1 + Math.Pow(2, -(((double)i / (maximumFrequencyIndex - minimumFrequencyIndex)) * 10 - 5))) + .5;

				//This is used later for calculating colors and positions
				Colors.Add(new HSLColor((double)(i - minimumFrequencyIndex) / (maximumFrequencyIndex - minimumFrequencyIndex), 1, fftBucketHeight * VolumeCorrection));
			}

			//Scale everything acording to the max volume value
			double MaxHeight = Colors.Max(x => x.Luminosity);
			if (MaxHeight > 0 && MaxHeight > 1)
				for (int i = 0; i < Colors.Count; i++)
					Colors[i].Luminosity /= MaxHeight;

			//Optional PeakDetection
			List<HSLColor> TopHSLValues = new List<HSLColor>();
			if (PeakDetectionEnabled)
			{
				Peaks = Sound_Library.BassEngine.PeakDetection(Colors.Select(x => x.Luminosity).ToList(), .01);
				for (int i = 0; i < Peaks.Count; i++)
					if (Peaks[i])
						TopHSLValues.Add(new HSLColor(Colors[i].Hue, 1, Colors[i].Luminosity));
			}
			else
				TopHSLValues = Colors;

			AverageColor = GetAVGHSLColor(TopHSLValues, ColorCalculationMode, BrightnessCalculationMode);

			//Send the color to any controllers
			ColorOut(AverageColor);
			//print the color we're using
			Console.WriteLine(AverageColor);
			//update out list of ColorsAndPeaks, which a control depends on
			ColorData = Colors;
		}

		/// <summary>
		/// Gets the Average HSL color from a list
		/// </summary>
		/// <param name="ColorIn">List of HSL colors to average</param>
		/// <returns>An average of all the HSL colors given</returns>
		private HSLColor GetAVGHSLColor(List<HSLColor> ColorIn, ColorCalculationModes color, BrightnessCalculationModes brightness)
		{
			HSLColor ret = new HSLColor(0.0, 1, 0);
			List<HSLColor> TempColors = new List<HSLColor>(ColorIn);
			if (TempColors.Count > 0)
			{

				switch (color)
				{
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

				switch (brightness)
				{
					case (BrightnessCalculationModes.Average):
						{
							ret.Luminosity = TempColors.Average(z => z.Luminosity);
							break;
						}
					case (BrightnessCalculationModes.Top10):
						{
							ret.Luminosity = TempColors.OrderByDescending(z => z.Luminosity).Take((int)Math.Round((TempColors.Count * .1) < .5 ? 1 : TempColors.Count * .1, MidpointRounding.AwayFromZero)).Average(z => z.Luminosity);
							break;
						}
					case (BrightnessCalculationModes.Top25):
						{
							ret.Luminosity = TempColors.OrderByDescending(z => z.Luminosity).Take((int)Math.Round((TempColors.Count * .25) < .5 ? 1 : TempColors.Count * .25, MidpointRounding.AwayFromZero)).Average(z => z.Luminosity);
							break;
						}
					case (BrightnessCalculationModes.Top50):
						{
							ret.Luminosity = TempColors.OrderByDescending(z => z.Luminosity).Take((int)Math.Round((TempColors.Count * .5) < .5 ? 1 : TempColors.Count * .5, MidpointRounding.AwayFromZero)).Average(z => z.Luminosity);
							break;
						}
				}

			}
			return ret;
		}
	}
}
