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

		private int _TabControlSelected;
		/// <summary>
		/// Index of the MainWindo TabControl
		/// </summary>
		public int TabControlSelected
		{
			get { return _TabControlSelected; }
			set
			{
				_TabControlSelected = value;

				NotifyPropertyChanged();
			}
		}


		public MainWindowViewModel()
		{

		}

		public void DisableUIElement(object o)
		{
			//disable spectrum when tabcontrol is not rendering it
			if (o is Spectrum s)
				s.AnimatingState(TabControlSelected == 1);
		}

	}
}
