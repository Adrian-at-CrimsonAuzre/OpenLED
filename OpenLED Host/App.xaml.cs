using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace OpenLED_Host
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		/// <summary>
		/// Used primarily to determine if MessageBoxes should be shown
		/// </summary>
		public bool IsGUIEnabled = true;

		protected override void OnStartup(StartupEventArgs e)
		{
			//do normal startup
			base.OnStartup(e);

			//if there are arguments, then launch the CLI version of application
			if (e.Args.Length == 0)
			{
				//Launch GUI
				ConsoleAllocator.HideConsoleWindow();
				new MainWindow().ShowDialog();
			}
			else
			{
				IsGUIEnabled = false;
				//Do command line stuff
				CLI cli = new CLI(e.Args);
			}
			Shutdown();
		}
	}
}
