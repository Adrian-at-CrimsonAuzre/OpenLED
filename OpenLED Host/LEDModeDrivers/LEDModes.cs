namespace OpenLED_Host.LEDModeDrivers
{
	public enum LEDModes
	{
		/// <summary>
		/// Turn LEDs off (technically the same as a StaticColor of 0,0,0)
		/// </summary>
		Off,

		/// <summary>
		/// Shines one color
		/// </summary>
		StaticColor,

		/// <summary>
		/// Fades in/out between two colors with a user specified delay
		/// </summary>
		Breathing,

		/// <summary>
		/// Thump Thump between two colors
		/// </summary>
		HeartBeat,

		/// <summary>
		/// Alternate two colors with no fade at user specified delay
		/// </summary>
		Strobe,

		/// <summary>
		/// Cycle between two colors on the HSL color space
		/// </summary>
		Cycle,

		/// <summary>
		/// Cycle through the rainbow
		/// </summary>
		Rainbow,

		/// <summary>
		/// Flash colors to the music
		/// </summary>
		ColorReactive,

		/// <summary>
		/// Display FFT data in colors, where bar height becomes brightness
		/// </summary>
		Visualizer
	}
}
