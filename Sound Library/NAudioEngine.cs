using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace Sound_Library
{
	public class NAudioEngine : OpenLED_Windows_Host.NotifyBase
	{
		#region Fields
		private bool _IsPlaying;
		/// <summary>
		/// Is audio playing?
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

		private static NAudioEngine _Instance;
		/// <summary>
		/// Instance of the NAduioEngine that is persistant throughout the application
		/// </summary>
		public static NAudioEngine Instance
		{
			get
			{
				if (_Instance == null)
					_Instance = new NAudioEngine();
				return _Instance;
			}
		}

		private FTTSize _FTTDataSize = FTTSize.FFT32768;
		/// <summary>
		/// FTT Size used throughout the application
		/// </summary>
		public FTTSize FTTDataSize
		{
			get
			{
				return _FTTDataSize;
			}
		}

		private IWaveIn WaveIn;
		private byte[] WASAPIBuffer;
		#endregion Fields

		private NAudioEngine()
		{
			// If we are currently record then go ahead and exit out.
			if (IsPlaying == true)
			{
				return;
			}
			WaveIn = new WasapiLoopbackCapture();
			WaveIn.DataAvailable += OnDataAvailable;
			WaveIn.RecordingStopped += OnRecordingStopped;
			WaveIn.StartRecording();
			IsPlaying = true;
		}

		/// <summary>
		/// Event handled when data becomes available.  The data will be written out to disk at this point.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnDataAvailable(object sender, WaveInEventArgs e)
		{
			WASAPIBuffer = new byte[e.BytesRecorded];
			e.Buffer.CopyTo(WASAPIBuffer, 0);
			//_writer.Write(e.Buffer, 0, e.BytesRecorded);
			//int secondsRecorded = (int)(_writer.Length / _writer.WaveFormat.AverageBytesPerSecond);
		}

		#region FFT
		public int GetFFTFrequencyIndex(int frequency)
		{
			return (int)FTTDataSize*frequency/44100;
		}

		public bool GetFFTData(float[] fftDataBuffer)
		{
			float[] StereoBuffer = new float[(int)FTTDataSize * 2];
			if (WASAPIBuffer.Count() > 0)
			{
				float[] YStereoBuffer = new float[(int)FTTDataSize * 2];
				float[] YBuffer = new float[(int)FTTDataSize];
				FFT(StereoBuffer, YStereoBuffer);

				//average the left and right channels
				for (int i = 0; i < (int)FTTDataSize; i++)
				{
					fftDataBuffer[i] = (StereoBuffer[i] + StereoBuffer[(int)FTTDataSize * 2 - i - 1]) / 2;
					YBuffer[i] = (YStereoBuffer[i] + YStereoBuffer[(int)FTTDataSize * 2 - i - 1]) / 2;
				}

				for (int i = 0; i < fftDataBuffer.Length / 2; i++)
				{
					// Calculate actual intensities for the FFT results.
					fftDataBuffer[i] = (float)Math.Sqrt(fftDataBuffer[i] * fftDataBuffer[i] + YStereoBuffer[i] * YStereoBuffer[i]) * 4;
				}

				return true;
			}
			else
				return false;
		}

		public void FFT(float[] X, float[] Y, bool forward = true)
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
		#endregion FFT

		/// <summary>
		/// Stops the recording
		/// </summary>
		public void StopRecording()
		{
			if (WaveIn == null)
			{
				return;
			}
			WaveIn.StopRecording();
		}

		/// <summary>
		/// Event handled when recording is stopped.  We will clean up open objects here that are required to be 
		/// closed and/or disposed of.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnRecordingStopped(object sender, StoppedEventArgs e)
		{
			if (WaveIn != null)
			{
				WaveIn.Dispose();
				WaveIn = null;
			}
			IsPlaying = false;
			if (e.Exception != null)
			{
				throw e.Exception;
			}
		}
	}
}
