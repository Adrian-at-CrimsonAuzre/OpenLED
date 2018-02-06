using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenLED_Host.Models;
using OpenLED_Host.LEDModeDrivers;
using OpenLED_Host.Controls;

namespace OpenLED_Host.ViewModels
{
	class MainWindowViewModel : NotifyBase
	{
		private VolumeAndPitchReactive _VolumeAndPitch = new VolumeAndPitchReactive();
		/// <summary>
		/// Backend for lighting
		/// </summary>
		public VolumeAndPitchReactive VolumeAndPitch
		{
			get { return _VolumeAndPitch; }
			set
			{
				_VolumeAndPitch = value;
				NotifyPropertyChanged();
			}
		}


		/// <summary>
		/// Visualizer being displayed to user, only reason it's here is to be able to access some of its methods
		/// </summary>
		public Spectrum Visualizer;


		private int _TabControlSelected = 0;
		/// <summary>
		/// Index of the MainWindo TabControl
		/// </summary>
		public int TabControlSelected
		{
			get { return _TabControlSelected; }
			set
			{
				//if current tab is 6 and new tab is not 6
				if (_TabControlSelected == 6 && value != 6)
					Visualizer.AnimatingState(false);
				else if (_TabControlSelected != 6 && value == 6)
					Visualizer.AnimatingState(true);
				_TabControlSelected = value;

				NotifyPropertyChanged();
			}
		}


		public MainWindowViewModel()
		{

		}
	}
}
