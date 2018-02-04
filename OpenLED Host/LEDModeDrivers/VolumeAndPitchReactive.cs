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
		private int _BlendedFrames = 3;
		/// <summary>
		/// Number of color frames that will be blended
		/// </summary>
		public int BlendedFrames
		{
			get { return _BlendedFrames; }
			set
			{
				_BlendedFrames = value;
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
			}
		}
		
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
				HSLColor t;
				GenerateColorData(out t);
				ticking = false;
			}
			else { }
		}

		public List<(double,int)> GenerateColorData(out HSLColor AVGColor)
		{
			float[] channelData = new float[(int)Sound_Library.BassEngine.Instance.FFT];
			Sound_Library.BassEngine.Instance.GetFFTData(channelData);

			List<(double v, int p)> VolumesAndPeaks = new List<(double, int)>();

			int scaleFactorLinear = 5;
			bool allzeros = true;

			int maximumFrequencyIndex = Sound_Library.BassEngine.Instance.GetFFTFrequencyIndex(MaximumFrequency);
			int minimumFrequencyIndex = Sound_Library.BassEngine.Instance.GetFFTFrequencyIndex(MinimumFrequency);


			for (int i = minimumFrequencyIndex; i <= maximumFrequencyIndex; i++)
			{
				//They need reset evert loop anyways
				double fftBucketHeight = 0f;

				if (channelData[i] > 1e-5)
					allzeros = false;

				fftBucketHeight = (channelData[i] * scaleFactorLinear);

				//Keep peaks in range incase weird shit has happened
				if (fftBucketHeight < 0f)
					fftBucketHeight = 0f;
				if (fftBucketHeight > 1)
					fftBucketHeight = 1;

				//This is used later for calculating colors and positions
				VolumesAndPeaks.Add((fftBucketHeight, 0));
			}

			//If all the data was zeros, then don't update
			if (!allzeros)
			{
				List<int> Peaks = Sound_Library.BassEngine.PeakDetection(VolumesAndPeaks.Select(x=>x.v).ToList());
				List<HSLColor> TopHSLValues = new List<HSLColor>();
				for (int i = 0; i < Peaks.Count; i++)
				{
					if (Peaks[i] > 0)
						TopHSLValues.Add(new HSLColor((double)i / VolumesAndPeaks.Count, 1, VolumesAndPeaks[i].v));
					VolumesAndPeaks[i] = (VolumesAndPeaks[i].v, Peaks[i]);
				}

				//add current color average to end of list, and remove extras if needed
				ColorsToBlend.Add(GetAVGHSLColor(TopHSLValues));
				if (ColorsToBlend.Count() > BlendedFrames + 1)
					for (int i = ColorsToBlend.Count(); i > BlendedFrames + 1; i--)
						ColorsToBlend.RemoveRange(0, 1);

				//average last [BlendedFrames] background colors together
				AVGColor = BlendColors();

				//Write color to all areas
				ColorOut(AVGColor);
			}
			else
			{
				ColorOut(new HSLColor(0, 0, 0));
				AVGColor = new HSLColor(0, 0, 0);
			}
			return VolumesAndPeaks;
		}
		/// <summary>
		/// Blends the colors that have been placed in the ColorsToBlend List
		/// </summary>
		/// <returns>A blended color</returns>
		public HSLColor BlendColors()
		{
			HSLColor ret = new HSLColor(0, 1, 0);

			for (int i = 0; i < ColorsToBlend.Count(); i++)
				ret = new HSLColor((ret.Hue * i + ColorsToBlend[i].Hue) / (i + 1), 1, (ret.Luminosity * i + ColorsToBlend[i].Luminosity) / (i + 1));

			return ret;
		}

		public static HSLColor GetAVGHSLColor(List<HSLColor> Colors)
		{

			double AVGH = 0;
			for (int i = 0; i < Colors.Count(); i++)
				if (Colors[i].Luminosity > 0)
					AVGH = (AVGH * i + Colors[i].Hue) / (i + 1);

			//Get a good Value for the background color that is related to how "loud" the sound is by selecting the top 10%
			HSLColor Average = new HSLColor(AVGH, 1, Colors.Count > 0 ? Colors.OrderByDescending(x => x.Luminosity).Take((int)(Math.Round(Colors.Count * .1) > 0 ? Math.Round(Colors.Count * .1) : Colors.Count)).Average(x => x.Luminosity) : 0);
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
