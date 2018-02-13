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
		/// <summary>
		/// Current LED Mode of the application
		/// </summary>
		public LEDModes LEDMode
		{
			get { return Properties.Settings.Default.LEDMode; }
			set
			{
				Properties.Settings.Default.LEDMode = value;
				Properties.Settings.Default.Save();

				NotifyPropertyChanged();
			}
		}

		/// <summary>
		/// Color One of various effects
		/// </summary>
		public HSLColor ColorOne
		{
			get { return Properties.Settings.Default.ColorOne; }
			set
			{
				Properties.Settings.Default.ColorOne = value;
				Properties.Settings.Default.Save();
				NotifyPropertyChanged();
			}
		}

		/// <summary>
		/// Color Two of various effects
		/// </summary>
		public HSLColor ColorTwo
		{
			get { return Properties.Settings.Default.ColorTwo; }
			set
			{
				Properties.Settings.Default.ColorTwo = value;
				Properties.Settings.Default.Save();
				NotifyPropertyChanged();
			}
		}

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

		private StaticColor _StaticColor = new StaticColor();
		/// <summary>
		/// The Static Color property
		/// </summary>
		public StaticColor StaticColor
		{
			get { return _StaticColor; }
			set
			{
				_StaticColor = value;
				NotifyPropertyChanged();
			}
		}
		
		private System.Windows.Controls.TabItem _TabControlSelected;
		/// <summary>
		/// Index of the MainWindo TabControl
		/// </summary>
		public System.Windows.Controls.TabItem TabControlSelected
		{
			get { return _TabControlSelected; }
			set
			{
				if(LEDMode == LEDModes.VolumeAndPitchReactive)
					//If the current item is the visualizer, and we're switching away from it, turn it off
					if ((string)value.Header != "Visualizer" && TabControlSelected != null && (string)TabControlSelected.Header == "Visualizer")
						(TabControlSelected.Content as Spectrum).AnimatingState(false);
					//If the new value is the visualizer, turn it on
					else if((string)value.Header == "Visualizer")
						(value.Content as Spectrum).AnimatingState(true);

				_TabControlSelected = value;

				NotifyPropertyChanged();
			}
		}


		public MainWindowViewModel()
		{

		}
	}
}
