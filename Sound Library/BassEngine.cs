using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using Un4seen.Bass;
using Un4seen.BassWasapi;

namespace Sound_Library
{
    public class BassEngine : OpenLED_Windows_Host.NotifyBase
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
    }
}
