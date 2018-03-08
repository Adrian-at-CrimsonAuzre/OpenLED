using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OpenLED_Host.Views
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon
		{
			Icon = new System.Drawing.Icon("Icon.ico"),
			Visible = false
		};

		public MainWindow()
		{
			InitializeComponent();
			
			ni.Click +=
				delegate (object sender, EventArgs args)
				{
					this.Show();
					this.WindowState = WindowState.Normal;
				};

			ColorPickerOne.Color = Properties.Settings.Default.ColorOne == null ? new HSLColor(0, 0.0, 0) : Properties.Settings.Default.ColorOne;
			ColorPickerTwo.Color = Properties.Settings.Default.ColorTwo == null ? new HSLColor(0, 0.0, 0) : Properties.Settings.Default.ColorTwo;

			Resources.MergedDictionaries.Clear();
			ResourceDictionary themeResources = Application.LoadComponent(new Uri("ExpressionDark.xaml", UriKind.Relative)) as ResourceDictionary;
			Resources.MergedDictionaries.Add(themeResources);
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			//MainWindowViewModel.VolumeAndPitch.StopReacting();
		}
		protected override void OnStateChanged(EventArgs e)
		{
			if (WindowState == WindowState.Minimized)
			{
				this.ShowInTaskbar = false;
				ni.Visible = true;
			}
			else
			{
				this.Activate();
				this.ShowInTaskbar = true;
				ni.Visible = false;
			}

			base.OnStateChanged(e);
		}

		//Since the Color Controls do not have working bindings...

		private void ColorPickerControl_OnPickColorOne(System.Drawing.Color color)
		{
			MainWindowViewModel.ColorOne = color;
		}
		private void ColorPickerControl_OnPickColorTwo(System.Drawing.Color color)
		{
			MainWindowViewModel.ColorTwo = color;
		}
	}
}
