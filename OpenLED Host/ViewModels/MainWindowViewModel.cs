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
		private LEDModes _LEDMode = LEDModes.NULL;
		/// <summary>
		/// Current LED Mode of the application
		/// </summary>
		public LEDModes LEDMode
		{
			get { return _LEDMode; }
			set
			{
				_LEDMode = value;

				switch(LEDMode)
				{
					case (LEDModes.VolumeAndPitchReactive):
						{
							VolumeAndPitch.StartReacting();
							break;
						}
					case (LEDModes.SingleColor):
						{
							VolumeAndPitch.StopReacting();
							break;
						}
				}

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

		private HSLColor _ColorOne = new HSLColor(0, 0, 0);
		/// <summary>
		/// Color One of various effects
		/// </summary>
		public HSLColor ColorOne
		{
			get { return _ColorOne; }
			set
			{
				_ColorOne = value;
				if (LEDMode == LEDModes.SingleColor)
					StaticColor.Color = ColorOne;
				NotifyPropertyChanged();
			}
		}

		private HSLColor _ColorTwo = new HSLColor(0, 0, 0);
		/// <summary>
		/// Color Two of various effects
		/// </summary>
		public HSLColor ColorTwo
		{
			get { return _ColorTwo; }
			set
			{
				_ColorTwo = value;
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
