using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Interop;
using Un4seen.Bass;
using Un4seen.BassWasapi;

namespace Sound_Library
{
    public class BassEngine
    {
        #region Fields
		public readonly FFTSize FFT = FFTSize.FFT32768;

		private static BassEngine _Instance;
		/// <summary>
		/// Application wide Bass Instance
		/// </summary>
		public static BassEngine Instance
		{
			get
			{
				if (_Instance == null)
					Instance = new BassEngine();
				return _Instance;
			}
			private set
			{
				_Instance = value;
			}
		}


		private bool _IsPlaying;
		/// <summary>
		/// Is there audio playing?
		/// </summary>
		public bool IsPlaying
		{
			get { return _IsPlaying; }
			set
			{
				_IsPlaying = value;
				NotifyPropertyChanged();
			}
		}

		#endregion

		private BassEngine()
        {
			//Setup Bass bullshit
			BassNet.Registration("trial@trial.com", "2X1837515183722");
			BassNet.OmitCheckVersion = true;
			Bass.LoadMe();
			Bass.BASS_Init(0, 48000, 0, IntPtr.Zero);
			BassWasapi.LoadMe();


			if (!Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_SPEAKERS, (new WindowInteropHelper(Application.Current.MainWindow)).Handle))
			{
				MessageBox.Show(Application.Current.MainWindow, "Bass initialization error!");
				Application.Current.MainWindow.Close();
			}

			#region WASAPI
			int WASAPIDeviceIndex = -100;

			var devices = BassWasapi.BASS_WASAPI_GetDeviceInfos();
			for (int i = 0; i < devices.Length; i++)
				if (devices[i].IsEnabled && devices[i].SupportsRecording && devices[i].IsLoopback)
				{
					WASAPIDeviceIndex = i;
					break;
				}
			if (WASAPIDeviceIndex != -100)
			{
				BASS_WASAPI_DEVICEINFO devInfo = devices[WASAPIDeviceIndex];

				if (devInfo.IsInitialized)
				{
					Console.WriteLine("Deinitializing WASAPI device");
					BassWasapi.BASS_WASAPI_Stop(true);
					BassWasapi.BASS_WASAPI_Free();
				}

				if (!BassWasapi.BASS_WASAPI_Init(WASAPIDeviceIndex, devInfo.mixfreq, devInfo.mixchans, BASSWASAPIInit.BASS_WASAPI_AUTOFORMAT | BASSWASAPIInit.BASS_WASAPI_BUFFER, 0f, 0f, new WASAPIPROC(delegate (IntPtr buffer, int length, IntPtr user) { return 1; }), IntPtr.Zero))
				{
					BASSError error = Bass.BASS_ErrorGetCode();
				}
				if (!BassWasapi.BASS_WASAPI_Start())
				{
					BASSError error = Bass.BASS_ErrorGetCode();
				}

				IsPlaying = true;
			}
			else
				IsPlaying = false;
			#endregion WASAPI
		}
		
		public int GetFFTFrequencyIndex(int frequency)
        {
			return Utils.FFTFrequency2Index(frequency, (int)FFT, 44100);
        }

        public bool GetFFTData(float[] fftDataBuffer)
        {			
			float[] StereoBuffer = new float[(int)FFT * 2];
			if ((BassWasapi.BASS_WASAPI_GetData(StereoBuffer, (int)FFT*2)) > 0)
			{
				float[] YStereoBuffer = new float[(int)FFT * 2];
				float[] YBuffer = new float[(int)FFT];
				FastFourierTransform(StereoBuffer, YStereoBuffer);

				//average the left and right channels
				for (int i = 0; i < (int)FFT; i++)
				{
					fftDataBuffer[i] = (StereoBuffer[i] + StereoBuffer[(int)FFT * 2 - i - 1]) / 2;
					YBuffer[i] = (YStereoBuffer[i] + YStereoBuffer[(int)FFT * 2 - i - 1]) / 2;
				}

				for (int i = 0; i < fftDataBuffer.Length/2; i++)
				{
					// Calculate actual intensities for the FFT results.
					fftDataBuffer[i] = (float)Math.Sqrt(fftDataBuffer[i] * fftDataBuffer[i] + YStereoBuffer[i] * YStereoBuffer[i])*4;
				}

				return true;
			}
			else
				return false;
        }

		public void FastFourierTransform(float[] X, float[] Y, bool forward=true)
		{
			int m = (int)Math.Log(X.Length, 2), n, i, i1, j, k, i2, l, l1, l2;
			float c1, c2, tx, ty, t1, t2, u1, u2, z;

			// Calculate the number of points
			n = 1;
			for (i = 0; i < m; i++)
				n *= 2;

			// Do the bit reversal
			i2 = n >> 1;
			j = 0;
			for (i = 0; i < n - 1; i++)
			{
				if (i < j)
				{
					tx = X[i];
					ty = Y[i];
					X[i] = X[j];
					Y[i] = Y[j];
					X[j] = tx;
					Y[j] = ty;
				}
				k = i2;

				while (k <= j)
				{
					j -= k;
					k >>= 1;
				}
				j += k;
			}

			// Compute the FFT 
			c1 = -1.0f;
			c2 = 0.0f;
			l2 = 1;
			for (l = 0; l < m; l++)
			{
				l1 = l2;
				l2 <<= 1;
				u1 = 1.0f;
				u2 = 0.0f;
				for (j = 0; j < l1; j++)
				{
					for (i = j; i < n; i += l2)
					{
						i1 = i + l1;
						t1 = u1 * X[i1] - u2 * Y[i1];
						t2 = u1 * Y[i1] + u2 * X[i1];
						X[i1] = X[i] - t1;
						Y[i1] = Y[i] - t2;
						X[i] += t1;
						Y[i] += t2;
					}
					z = u1 * c1 - u2 * c2;
					u2 = u1 * c2 + u2 * c1;
					u1 = z;
				}
				c2 = (float)Math.Sqrt((1.0f - c1) / 2.0f);
				if (forward)
					c2 = -c2;
				c1 = (float)Math.Sqrt((1.0f + c1) / 2.0f);
			}

			// Scaling for forward transform 
			if (forward)
			{
				for (i = 0; i < n; i++)
				{
					X[i] /= n;
					Y[i] /= n;
				}
			}
		}

		/// <summary>
		/// Detects peaks out of data
		/// </summary>
		/// <param name="y">Values to perform peak detection</param>
		/// <param name="ThresholdWindow">Window to search for peaks in, default of 5</param>
		/// <param name="ThresholdThreshold">Filter correction value, default of .1</param>
		/// <param name="ThresholdInfluence">Changes falloff of detected peak, default of .5</param>
		/// <returns>List of values (-1, 0, 1) for peak, average, and trough</returns>
		public static List<int> PeakDetection(List<double> y, int ThresholdWindow = 5, double ThresholdThreshold = 0.1, double ThresholdInfluence = .5)
		{
			try
			{
				//Add pad to front of y so that initial values can be picked up. Adding /these/ values to the front helps the algorithm build a better picture of the first peak (instead of selecting everything before the first peak)
				List<double> InvertedThreshold = new List<double>(y.GetRange(0, +1));
				for (int i = 0; i <= ThresholdWindow; i++)
					y.Insert(0, y[i + i - (i == 0 ? 0 : 1)]);

				//set these up to the right size
				List<int> signals = new List<int>(Enumerable.Repeat(0, y.Count));
				List<double> FilteredY = new List<double>(Enumerable.Repeat(0.0, y.Count));
				List<double> avgFilter = new List<double>(Enumerable.Repeat(0.0, y.Count));
				List<double> stdFilter = new List<double>(Enumerable.Repeat(0.0, y.Count));

				//Copy the section of data over
				List<double> subList = new List<double>(Enumerable.Repeat(0.0, ThresholdWindow));
				for (int i = 0; i < ThresholdWindow; i++)
					subList[i] = y[i];

				//Get our first datapoint
				avgFilter[ThresholdWindow] = subList.Average();
				stdFilter[ThresholdWindow] = StandardDeviation(subList);

				for (int i = ThresholdWindow + 1; i < y.Count; i++)
				{
					if (Math.Abs(y[i] - avgFilter[i - 1]) > ThresholdThreshold * stdFilter[i - 1])
					{
						if (y[i] > avgFilter[i - 1])
							signals[i] = 1;
						else
							signals[i] = -1;
						FilteredY[i] = ThresholdInfluence * y[i] + (1 - ThresholdInfluence) * FilteredY[i - 1];
					}
					else
					{
						signals[i] = 0;
						FilteredY[i] = y[i];
					}

					//remake sublist
					for (int j = i - ThresholdWindow; j < i; j++)
						subList.Add(FilteredY[i]);

					avgFilter[i] = subList.Average();
					stdFilter[i] = StandardDeviation(subList);
				}

				//remove pad
				for (int i = 0; i <= ThresholdWindow; i++)
					signals.Remove(0);

				return signals;
			}
			catch
			{
				//if break, return empty list
				return new List<int>(Enumerable.Repeat(0, y.Count));
			}
		}

		/// <summary>
		/// Finds standard deviation of a list of doubles
		/// </summary>
		/// <param name="values">List to find deviation of</param>
		/// <returns>Standard deviation of a dataset</returns>
		private static double StandardDeviation(List<double> values)
		{
			var g = values.Aggregate(new { mean = 0.0, sum = 0.0, count = 0 },
			(acc, val) =>
			{
				var newcount = acc.count + 1;
				double delta = val - acc.mean;
				var newmean = acc.mean + delta / newcount;
				return new { mean = newmean, sum = acc.sum + delta * (val - newmean), count = newcount };
			});
			return Math.Sqrt(g.sum / g.count);
		}


		//base for INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;
		public virtual void NotifyPropertyChanged([CallerMemberName]string member = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(member));
		}
	}
}
