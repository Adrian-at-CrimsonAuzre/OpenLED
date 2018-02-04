using System;
using System.Runtime.InteropServices;

namespace OpenLED_Host
{
	internal static class ConsoleAllocator
	{
		[DllImport(@"kernel32.dll", SetLastError = true)]
		static extern bool AllocConsole();

		[DllImport(@"kernel32.dll")]
		static extern IntPtr GetConsoleWindow();

		[DllImport(@"user32.dll")]
		static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		const int SwHide = 0;
		const int SwShow = 5;

		/// <summary>
		/// Shows application console window
		/// </summary>
		public static void ShowConsoleWindow()
		{
			try
			{
				var handle = GetConsoleWindow();

				if (handle == IntPtr.Zero)
				{
					AllocConsole();
				}
				else
				{
					ShowWindow(handle, SwShow);
				}
			}
			catch { }
		}
		/// <summary>
		/// Hides application console window
		/// </summary>
		public static void HideConsoleWindow()
		{
			try
			{
				var handle = GetConsoleWindow();

				ShowWindow(handle, SwHide);
			}
			catch { }
		}
	}
}
