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
		public LEDModeDrivers.VolumeAndPitchReactive VP
		{
			get;
			set;
		} = new LEDModeDrivers.VolumeAndPitchReactive();
		public MainWindow()
		{
			InitializeComponent();
			VP.StartReacting();
			Resources.MergedDictionaries.Clear();
			ResourceDictionary themeResources = Application.LoadComponent(new Uri("ExpressionDark.xaml", UriKind.Relative)) as ResourceDictionary;
			Resources.MergedDictionaries.Add(themeResources);
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			VP.StopReacting();
		}
	}
}
