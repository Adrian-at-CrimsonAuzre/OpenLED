using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Linq;

namespace OpenLED_Host.Controls
{

	/// <summary>
	/// A spectrum analyzer control for visualizing audio level and frequency data.
	/// </summary>
	[DisplayName("Spectrum Analyzer")]
	[Description("Displays audio level and frequency data.")]
	[ToolboxItem(true)]
	[TemplatePart(Name = "PART_SpectrumCanvas", Type = typeof(Canvas))]
	public class Spectrum : Control
	{
		#region Fields
		private readonly DispatcherTimer animationTimer;
		private Canvas spectrumCanvas;
		private double barWidth = 1;
		private int BarCount;
		#endregion

		#region Dependency Properties
		
		#region BarVisibility
		/// <summary>
		/// Identifies the <see cref="BarVisibility" /> dependency property. 
		/// </summary>
		public static readonly DependencyProperty BarVisibilityProperty = DependencyProperty.Register("BarVisibility", typeof(Visibility), typeof(Spectrum), new UIPropertyMetadata(Visibility.Hidden, OnBarVisibilityChanged, OnCoerceBarVisibility));

		private static object OnCoerceBarVisibility(DependencyObject o, object value)
		{
			Spectrum spectrum = o as Spectrum;
			if (spectrum != null)
				return spectrum.OnCoerceBarVisibility((Visibility)value);
			else
				return value;
		}

		private static void OnBarVisibilityChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			Spectrum spectrum = o as Spectrum;
			if (spectrum != null)
				spectrum.OnBarVisibilityChanged((Visibility)e.OldValue, (Visibility)e.NewValue);
		}

		/// <summary>
		/// Coerces the value of <see cref="BarVisibility"/> when a new value is applied.
		/// </summary>
		/// <param name="value">The value that was set on <see cref="BarVisibility"/></param>
		/// <returns>The adjusted value of <see cref="BarVisibility"/></returns>
		protected virtual Visibility OnCoerceBarVisibility(Visibility value)
		{
			return value;
		}

		/// <summary>
		/// Called after the <see cref="BarVisibility"/> value has changed.
		/// </summary>
		/// <param name="oldValue">The previous value of <see cref="BarVisibility"/></param>
		/// <param name="newValue">The new value of <see cref="BarVisibility"/></param>
		protected virtual void OnBarVisibilityChanged(Visibility oldValue, Visibility newValue)
		{
			UpdateBarLayout();
		}

		/// <summary>
		/// Gets or sets a value indicating whether each bar's peak 
		/// value will be averaged with the previous bar's peak.
		/// This creates a smoothing effect on the bars.
		/// </summary>
		[Category("Common")]
		public Visibility BarVisibility
		{
			// IMPORTANT: To maintain parity between setting a property in XAML and procedural code, do not touch the getter and setter inside this dependency property!
			get
			{
				return (Visibility)GetValue(BarVisibilityProperty);
			}
			set
			{
				SetValue(BarVisibilityProperty, value);
			}
		}
		#endregion
		
		#region BarStyle
		/// <summary>
		/// Identifies the <see cref="BarStyle" /> dependency property. 
		/// </summary>
		public static readonly DependencyProperty BarStyleProperty = DependencyProperty.Register("BarStyle", typeof(Style), typeof(Spectrum), new UIPropertyMetadata(null, OnBarStyleChanged, OnCoerceBarStyle));

		private static object OnCoerceBarStyle(DependencyObject o, object value)
		{
			Spectrum spectrum = o as Spectrum;
			if (spectrum != null)
				return spectrum.OnCoerceBarStyle((Style)value);
			else
				return value;
		}

		private static void OnBarStyleChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			Spectrum spectrum = o as Spectrum;
			if (spectrum != null)
				spectrum.OnBarStyleChanged((Style)e.OldValue, (Style)e.NewValue);
		}

		/// <summary>
		/// Coerces the value of <see cref="BarStyle"/> when a new value is applied.
		/// </summary>
		/// <param name="value">The value that was set on <see cref="BarStyle"/></param>
		/// <returns>The adjusted value of <see cref="BarStyle"/></returns>
		protected virtual Style OnCoerceBarStyle(Style value)
		{
			return value;
		}

		/// <summary>
		/// Called after the <see cref="BarStyle"/> value has changed.
		/// </summary>
		/// <param name="oldValue">The previous value of <see cref="BarStyle"/></param>
		/// <param name="newValue">The new value of <see cref="BarStyle"/></param>
		protected virtual void OnBarStyleChanged(Style oldValue, Style newValue)
		{
			UpdateBarLayout();
		}

		/// <summary>
		/// Gets or sets a style with which to draw the bars on the spectrum analyzer.
		/// </summary>
		public Style BarStyle
		{
			// IMPORTANT: To maintain parity between setting a property in XAML and procedural code, do not touch the getter and setter inside this dependency property!
			get
			{
				return (Style)GetValue(BarStyleProperty);
			}
			set
			{
				SetValue(BarStyleProperty, value);
			}
		}
		#endregion
		
		#region ActualBarWidth
		/// <summary>
		/// Identifies the <see cref="ActualBarWidth" /> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ActualBarWidthProperty = DependencyProperty.Register("ActualBarWidth", typeof(double), typeof(Spectrum), new UIPropertyMetadata(0.0d, OnActualBarWidthChanged, OnCoerceActualBarWidth));

		private static object OnCoerceActualBarWidth(DependencyObject o, object value)
		{
			Spectrum spectrum = o as Spectrum;
			if (spectrum != null)
				return spectrum.OnCoerceActualBarWidth((double)value);
			else
				return value;
		}

		private static void OnActualBarWidthChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			Spectrum spectrum = o as Spectrum;
			if (spectrum != null)
				spectrum.OnActualBarWidthChanged((double)e.OldValue, (double)e.NewValue);
		}

		/// <summary>
		/// Coerces the value of <see cref="ActualBarWidth"/> when a new value is applied.
		/// </summary>
		/// <param name="value">The value that was set on <see cref="ActualBarWidth"/></param>
		/// <returns>The adjusted value of <see cref="ActualBarWidth"/></returns>
		protected virtual double OnCoerceActualBarWidth(double value)
		{
			return value;
		}

		/// <summary>
		/// Called after the <see cref="ActualBarWidth"/> value has changed.
		/// </summary>
		/// <param name="oldValue">The previous value of <see cref="ActualBarWidth"/></param>
		/// <param name="newValue">The new value of <see cref="ActualBarWidth"/></param>
		protected virtual void OnActualBarWidthChanged(double oldValue, double newValue)
		{

		}

		/// <summary>
		/// Gets the actual width that the bars will be drawn at.
		/// </summary>
		public double ActualBarWidth
		{
			// IMPORTANT: To maintain parity between setting a property in XAML and procedural code, do not touch the getter and setter inside this dependency property!
			get
			{
				return (double)GetValue(ActualBarWidthProperty);
			}
			protected set
			{
				SetValue(ActualBarWidthProperty, value);
			}
		}
		#endregion

		#region VolumeAndPitch
		/// <summary>
		/// Identifies the <see cref="VolumeAndPitch" /> dependency property. 
		/// </summary>
		public static readonly DependencyProperty VolumeAndPitchProperty = DependencyProperty.Register("VolumeAndPitch", typeof(LEDModeDrivers.VolumeAndPitchReactive), typeof(Spectrum), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnVolumeAndPitchChanged, OnCoerceVolumeAndPitch));

		private static object OnCoerceVolumeAndPitch(DependencyObject o, object value)
		{
			Spectrum spectrum = o as Spectrum;
			if (spectrum != null)
				return spectrum.OnCoerceVolumeAndPitch((LEDModeDrivers.VolumeAndPitchReactive)value);
			else
				return value;
		}

		private static void OnVolumeAndPitchChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			Spectrum spectrum = o as Spectrum;
			if (spectrum != null)
				spectrum.OnVolumeAndPitchChanged((LEDModeDrivers.VolumeAndPitchReactive)e.OldValue, (LEDModeDrivers.VolumeAndPitchReactive)e.NewValue);
		}

		/// <summary>
		/// Coerces the value of <see cref="VolumeAndPitch"/> when a new value is applied.
		/// </summary>
		/// <param name="value">The value that was set on <see cref="VolumeAndPitch"/></param>
		/// <returns>The adjusted value of <see cref="VolumeAndPitch"/></returns>
		protected virtual LEDModeDrivers.VolumeAndPitchReactive OnCoerceVolumeAndPitch(LEDModeDrivers.VolumeAndPitchReactive value)
		{
			return value;
		}

		/// <summary>
		/// Called after the <see cref="VolumeAndPitch"/> value has changed.
		/// </summary>
		/// <param name="oldValue">The previous value of <see cref="VolumeAndPitch"/></param>
		/// <param name="newValue">The new value of <see cref="VolumeAndPitch"/></param>
		protected virtual void OnVolumeAndPitchChanged(LEDModeDrivers.VolumeAndPitchReactive oldValue, LEDModeDrivers.VolumeAndPitchReactive newValue)
		{
			UpdateBarLayout();
		}

		/// <summary>
		/// Gets or sets the maximum display frequency (right side) for the spectrum analyzer.
		/// </summary>
		/// <remarks>In usual practice, this value should be somewhere between 0 and half of the maximum sample rate. If using
		/// the maximum sample rate, this would be roughly 22000.</remarks>
		[Category("Common")]
		public LEDModeDrivers.VolumeAndPitchReactive VolumeAndPitch
		{
			// IMPORTANT: To maLEDModeDrivers.VolumeAndPitchReactiveain parity between setting a property in XAML and procedural code, do not touch the getter and setter inside this dependency property!
			get
			{
				return (LEDModeDrivers.VolumeAndPitchReactive)GetValue(VolumeAndPitchProperty);
			}
			set
			{
				SetValue(VolumeAndPitchProperty, value);
			}
		}
		#endregion

		#endregion

		#region Template Overrides
		/// <summary>
		/// When overridden in a derived class, is invoked whenever application code
		/// or internal processes call System.Windows.FrameworkElement.ApplyTemplate().
		/// </summary>
		public override void OnApplyTemplate()
		{
			spectrumCanvas = GetTemplateChild("PART_SpectrumCanvas") as Canvas;
			UpdateBarLayout();
		}

		/// <summary>
		/// Called whenever the control's template changes. 
		/// </summary>
		/// <param name="oldTemplate">The old template</param>
		/// <param name="newTemplate">The new template</param>
		protected override void OnTemplateChanged(ControlTemplate oldTemplate, ControlTemplate newTemplate)
		{
			base.OnTemplateChanged(oldTemplate, newTemplate);
		}
		#endregion

		#region Constructors
		static Spectrum()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(Spectrum), new FrameworkPropertyMetadata(typeof(Spectrum)));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Spectrum"/> class.
		/// </summary>
		public Spectrum()
		{
			animationTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle)
			{
				Interval = TimeSpan.FromMilliseconds(10),
			};
			animationTimer.Tick += animationTimer_Tick;

			Sound_Library.BassEngine.Instance.PropertyChanged += soundPlayer_PropertyChanged;
		}
		#endregion


		#region Event Overrides
		/// <summary>
		/// When overridden in a derived class, participates in rendering operations that are directed by the layout system. 
		/// The rendering instructions for this element are not used directly when this method is invoked, and are 
		/// instead preserved for later asynchronous use by layout and drawing.
		/// </summary>
		/// <param name="dc">The drawing instructions for a specific element. This context is provided to the layout system.</param>
		protected override void OnRender(DrawingContext dc)
		{
			base.OnRender(dc);
			UpdateBarLayout();
		}
		#endregion

		/// <summary>
		/// Starts or Stops animating the Spectrum Control
		/// </summary>
		/// <param name="ShouldAnimate">Should we be animating?</param>
		public void AnimatingState(bool ShouldAnimate)
		{
			//Only do things if the state is different than our current state
			if(ShouldAnimate != animationTimer.IsEnabled)
				if (!ShouldAnimate)
				{
					animationTimer.Stop();
					foreach (var c in spectrumCanvas.Children)
						if (c is Rectangle r)
							r.Height = 0;
					spectrumCanvas.Background = new SolidColorBrush(Colors.Black);
				}
				else
					animationTimer.Start();
		}

		#region Private Drawing Methods
		private void UpdateSpectrum()
		{

			if (spectrumCanvas == null || spectrumCanvas.RenderSize.Width < 1 || spectrumCanvas.RenderSize.Height < 1)
				return;

			if (!Sound_Library.BassEngine.Instance.IsPlaying)
				return;

			UpdateSpectrumShapes();
		}
		
		private void UpdateSpectrumShapes()
		{
			List<HSLColor> Colors = new List<HSLColor>(VolumeAndPitch.ColorData);
			HSLColor AverageColor = VolumeAndPitch.AverageColor;

			//If nothing is playing, then don't update
			if (AverageColor != new HSLColor(0, 0, 0) && Colors.Count > 0)
			{
				for (int i = 0; i < BarCount; i++)
				{
					double barHeight = Colors[i].Luminosity * spectrumCanvas.RenderSize.Height;
					
					((Rectangle)spectrumCanvas.Children[i]).Margin = new Thickness((barWidth * i) + 1, (spectrumCanvas.RenderSize.Height - 1) - barHeight, 0, 0);
					((Rectangle)spectrumCanvas.Children[i]).Height = barHeight;
					
					//Set color for the rectangles
					System.Drawing.Color RGB = new HSLColor((double)i / BarCount, 1, Colors[i].Luminosity);
					((Rectangle)spectrumCanvas.Children[i]).Fill = new SolidColorBrush(Color.FromRgb(RGB.R, RGB.G, RGB.B));
				}				

				//convert for solid brush
				Color AVG_RGB = Color.FromArgb(255, AverageColor.ToColor().R, AverageColor.ToColor().G, AverageColor.ToColor().B);
				spectrumCanvas.Background = new SolidColorBrush(Color.FromRgb(AVG_RGB.R, AVG_RGB.G, AVG_RGB.B));

				//Display stats
			}
			else
			{
				for (int i = 0; i < BarCount; i++)
				{
					((Rectangle)spectrumCanvas.Children[i]).Margin = new Thickness((barWidth * i) + 1, (spectrumCanvas.RenderSize.Height - 1), 0, 0);
					((Rectangle)spectrumCanvas.Children[i]).Height = 0;					
					((Rectangle)spectrumCanvas.Children[i]).StrokeThickness = 0;
				}
				spectrumCanvas.Background = new SolidColorBrush(System.Windows.Media.Colors.Black);
			}

			if (!Sound_Library.BassEngine.Instance.IsPlaying)
				animationTimer.Stop();
		}
		private void UpdateBarLayout()
		{
			if(VolumeAndPitch != null)
				BarCount = Sound_Library.BassEngine.Instance.GetFFTFrequencyIndex(VolumeAndPitch.MaximumFrequency) - Sound_Library.BassEngine.Instance.GetFFTFrequencyIndex(VolumeAndPitch.MinimumFrequency);
			if (spectrumCanvas == null || spectrumCanvas.RenderSize.IsEmpty || BarCount == 0)
				return;

			barWidth = Math.Max(spectrumCanvas.RenderSize.Width / BarCount, 1);			

			spectrumCanvas.Children.Clear();

			for (int i = 0; i < BarCount; i++)
			{
				double xCoord = (barWidth * i) + 1;
				Rectangle barRectangle = new Rectangle()
				{
					Margin = new Thickness(xCoord, spectrumCanvas.RenderSize.Height, 0, 0),
					Width = barWidth,
					Height = 0,
					Style = BarStyle,
					Visibility = BarVisibility,
					Stroke = new SolidColorBrush(Colors.White),
					StrokeThickness = 0
				};
				spectrumCanvas.Children.Add(barRectangle);
			}
			
			ActualBarWidth = barWidth;
		}
		#endregion

		#region Event Handlers
		private void soundPlayer_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "IsPlaying":
					if (Sound_Library.BassEngine.Instance.IsPlaying && !animationTimer.IsEnabled)
						animationTimer.Start();
					break;
			}
		}

		private void animationTimer_Tick(object sender, EventArgs e)
		{
			UpdateSpectrum();
		}
		#endregion
	}
}