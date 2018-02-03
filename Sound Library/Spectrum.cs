using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Linq;
using OpenLED_Windows_Host;

namespace Sound_Library
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
		private double[] barHeights;
		private float[] channelData = new float[(int)BassEngine.Instance.FFT];
		private double bandWidth = 1.0;
		private double barWidth = 1;
		private int maximumFrequencyIndex = (int)BassEngine.Instance.FFT - 1;
		private int minimumFrequencyIndex;
		private int[] barIndexMax;
		private int[] barLogScaleIndexMax;
		#endregion

		#region Constants
		private const int scaleFactorLinear = 22;
		private const int scaleFactorSqr = 2;
		private const double minDBValue = -90;
		private const double maxDBValue = 0;
		private const double dbScale = (maxDBValue - minDBValue);
		private const int defaultUpdateInterval = 0;
		#endregion

		#region Dependency Properties

		#region MaximumFrequency
		/// <summary>
		/// Identifies the <see cref="MaximumFrequency" /> dependency property. 
		/// </summary>
		public static readonly DependencyProperty MaximumFrequencyProperty = DependencyProperty.Register("MaximumFrequency", typeof(int), typeof(Spectrum), new UIPropertyMetadata(20000, OnMaximumFrequencyChanged, OnCoerceMaximumFrequency));

		private static object OnCoerceMaximumFrequency(DependencyObject o, object value)
		{
			Spectrum spectrum = o as Spectrum;
			if (spectrum != null)
				return spectrum.OnCoerceMaximumFrequency((int)value);
			else
				return value;
		}

		private static void OnMaximumFrequencyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			Spectrum spectrum = o as Spectrum;
			if (spectrum != null)
				spectrum.OnMaximumFrequencyChanged((int)e.OldValue, (int)e.NewValue);
		}

		/// <summary>
		/// Coerces the value of <see cref="MaximumFrequency"/> when a new value is applied.
		/// </summary>
		/// <param name="value">The value that was set on <see cref="MaximumFrequency"/></param>
		/// <returns>The adjusted value of <see cref="MaximumFrequency"/></returns>
		protected virtual int OnCoerceMaximumFrequency(int value)
		{
			if ((int)value < MinimumFrequency)
				return MinimumFrequency + 1;
			return value;
		}

		/// <summary>
		/// Called after the <see cref="MaximumFrequency"/> value has changed.
		/// </summary>
		/// <param name="oldValue">The previous value of <see cref="MaximumFrequency"/></param>
		/// <param name="newValue">The new value of <see cref="MaximumFrequency"/></param>
		protected virtual void OnMaximumFrequencyChanged(int oldValue, int newValue)
		{
			UpdateBarLayout();
		}

		/// <summary>
		/// Gets or sets the maximum display frequency (right side) for the spectrum analyzer.
		/// </summary>
		/// <remarks>In usual practice, this value should be somewhere between 0 and half of the maximum sample rate. If using
		/// the maximum sample rate, this would be roughly 22000.</remarks>
		[Category("Common")]
		public int MaximumFrequency
		{
			// IMPORTANT: To maintain parity between setting a property in XAML and procedural code, do not touch the getter and setter inside this dependency property!
			get
			{
				return (int)GetValue(MaximumFrequencyProperty);
			}
			set
			{
				SetValue(MaximumFrequencyProperty, value);
			}
		}
		#endregion

		#region Minimum Frequency
		/// <summary>
		/// Identifies the <see cref="MinimumFrequency" /> dependency property. 
		/// </summary>
		public static readonly DependencyProperty MinimumFrequencyProperty = DependencyProperty.Register("MinimumFrequency", typeof(int), typeof(Spectrum), new UIPropertyMetadata(20, OnMinimumFrequencyChanged, OnCoerceMinimumFrequency));

		private static object OnCoerceMinimumFrequency(DependencyObject o, object value)
		{
			Spectrum spectrum = o as Spectrum;
			if (spectrum != null)
				return spectrum.OnCoerceMinimumFrequency((int)value);
			else
				return value;
		}

		private static void OnMinimumFrequencyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			Spectrum spectrum = o as Spectrum;
			if (spectrum != null)
				spectrum.OnMinimumFrequencyChanged((int)e.OldValue, (int)e.NewValue);
		}

		/// <summary>
		/// Coerces the value of <see cref="MinimumFrequency"/> when a new value is applied.
		/// </summary>
		/// <param name="value">The value that was set on <see cref="MinimumFrequency"/></param>
		/// <returns>The adjusted value of <see cref="MinimumFrequency"/></returns>
		protected virtual int OnCoerceMinimumFrequency(int value)
		{
			if (value < 0)
				return value = 0;
			CoerceValue(MaximumFrequencyProperty);
			return value;
		}

		/// <summary>
		/// Called after the <see cref="MinimumFrequency"/> value has changed.
		/// </summary>
		/// <param name="oldValue">The previous value of <see cref="MinimumFrequency"/></param>
		/// <param name="newValue">The new value of <see cref="MinimumFrequency"/></param>
		protected virtual void OnMinimumFrequencyChanged(int oldValue, int newValue)
		{
			UpdateBarLayout();
		}

		/// <summary>
		/// Gets or sets the minimum display frequency (left side) for the spectrum analyzer.
		/// </summary>
		[Category("Common")]
		public int MinimumFrequency
		{
			// IMPORTANT: To maintain parity between setting a property in XAML and procedural code, do not touch the getter and setter inside this dependency property!
			get
			{
				return (int)GetValue(MinimumFrequencyProperty);
			}
			set
			{
				SetValue(MinimumFrequencyProperty, value);
			}
		}

		#endregion

		#region BarCount
		/// <summary>
		/// Identifies the <see cref="BarCount" /> dependency property. 
		/// </summary>
		public static readonly DependencyProperty BarCountProperty = DependencyProperty.Register("BarCount", typeof(int), typeof(Spectrum), new UIPropertyMetadata(32, OnBarCountChanged, OnCoerceBarCount));

		private static object OnCoerceBarCount(DependencyObject o, object value)
		{
			Spectrum spectrum = o as Spectrum;
			if (spectrum != null)
				return spectrum.OnCoerceBarCount((int)value);
			else
				return value;
		}

		private static void OnBarCountChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			Spectrum spectrum = o as Spectrum;
			if (spectrum != null)
				spectrum.OnBarCountChanged((int)e.OldValue, (int)e.NewValue);
		}

		/// <summary>
		/// Coerces the value of <see cref="BarCount"/> when a new value is applied.
		/// </summary>
		/// <param name="value">The value that was set on <see cref="BarCount"/></param>
		/// <returns>The adjusted value of <see cref="BarCount"/></returns>
		protected virtual int OnCoerceBarCount(int value)
		{
			value = Math.Max(value, 1);
			return value;
		}

		/// <summary>
		/// Called after the <see cref="BarCount"/> value has changed.
		/// </summary>
		/// <param name="oldValue">The previous value of <see cref="BarCount"/></param>
		/// <param name="newValue">The new value of <see cref="BarCount"/></param>
		protected virtual void OnBarCountChanged(int oldValue, int newValue)
		{
			UpdateBarLayout();
		}

		/// <summary>
		/// Gets or sets the number of bars to show on the sprectrum analyzer.
		/// </summary>
		/// <remarks>A bar's width can be a minimum of 1 pixel. If the BarSpacing and BarCount property result
		/// in the bars being wider than the chart itself, the BarCount will automatically scale down.</remarks>
		[Category("Common")]
		public int BarCount
		{
			// IMPORTANT: To maintain parity between setting a property in XAML and procedural code, do not touch the getter and setter inside this dependency property!
			get
			{
				return (int)GetValue(BarCountProperty);
			}
			set
			{
				SetValue(BarCountProperty, value);
			}
		}
		#endregion

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

		#region BlendedFrames
		/// <summary>
		/// Identifies the <see cref="BlendedFrames" /> dependency property. 
		/// </summary>
		public static readonly DependencyProperty BlendedFramesProperty = DependencyProperty.Register("BlendedFrames", typeof(int), typeof(Spectrum), new UIPropertyMetadata(0, OnBlendedFramesChanged, OnCoerceBlendedFrames));

		private static object OnCoerceBlendedFrames(DependencyObject o, object value)
		{
			Spectrum spectrum = o as Spectrum;
			if (spectrum != null)
				return spectrum.OnCoerceBlendedFrames((int)value);
			else
				return value;
		}

		private static void OnBlendedFramesChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			Spectrum spectrum = o as Spectrum;
			if (spectrum != null)
				spectrum.OnBlendedFramesChanged((int)e.OldValue, (int)e.NewValue);
		}

		/// <summary>
		/// Coerces the value of <see cref="BlendedFrames"/> when a new value is applied.
		/// </summary>
		/// <param name="value">The value that was set on <see cref="BlendedFrames"/></param>
		/// <returns>The adjusted value of <see cref="BlendedFrames"/></returns>
		protected virtual int OnCoerceBlendedFrames(int value)
		{
			value = Math.Max(value, 0);
			return value;
		}

		/// <summary>
		/// Called after the <see cref="BlendedFrames"/> value has changed.
		/// </summary>
		/// <param name="oldValue">The previous value of <see cref="BlendedFrames"/></param>
		/// <param name="newValue">The new value of <see cref="BlendedFrames"/></param>
		protected virtual void OnBlendedFramesChanged(int oldValue, int newValue)
		{
			UpdateBarLayout();
		}

		/// <summary>
		/// Gets or sets the number of bars to show on the sprectrum analyzer.
		/// </summary>
		/// <remarks>A bar's width can be a minimum of 1 pixel. If the BarSpacing and BlendedFrames property result
		/// in the bars being wider than the chart itself, the BlendedFrames will automatically scale down.</remarks>
		[Category("Common")]
		public int BlendedFrames
		{
			// IMPORTANT: To maintain parity between setting a property in XAML and procedural code, do not touch the getter and setter inside this dependency property!
			get
			{
				return (int)GetValue(BlendedFramesProperty);
			}
			set
			{
				SetValue(BlendedFramesProperty, value);
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

		#region RefreshRate
		/// <summary>
		/// Identifies the <see cref="RefreshInterval" /> dependency property. 
		/// </summary>
		public static readonly DependencyProperty RefreshIntervalProperty = DependencyProperty.Register("RefreshInterval", typeof(int), typeof(Spectrum), new UIPropertyMetadata(defaultUpdateInterval, OnRefreshIntervalChanged, OnCoerceRefreshInterval));

		private static object OnCoerceRefreshInterval(DependencyObject o, object value)
		{
			Spectrum spectrum = o as Spectrum;
			if (spectrum != null)
				return spectrum.OnCoerceRefreshInterval((int)value);
			else
				return value;
		}

		private static void OnRefreshIntervalChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			Spectrum spectrum = o as Spectrum;
			if (spectrum != null)
				spectrum.OnRefreshIntervalChanged((int)e.OldValue, (int)e.NewValue);
		}

		/// <summary>
		/// Coerces the value of <see cref="RefreshInterval"/> when a new value is applied.
		/// </summary>
		/// <param name="value">The value that was set on <see cref="RefreshInterval"/></param>
		/// <returns>The adjusted value of <see cref="RefreshInterval"/></returns>
		protected virtual int OnCoerceRefreshInterval(int value)
		{
			value = Math.Min(1000, Math.Max(1, value));
			return value;
		}

		/// <summary>
		/// Called after the <see cref="RefreshInterval"/> value has changed.
		/// </summary>
		/// <param name="oldValue">The previous value of <see cref="RefreshInterval"/></param>
		/// <param name="newValue">The new value of <see cref="RefreshInterval"/></param>
		protected virtual void OnRefreshIntervalChanged(int oldValue, int newValue)
		{
			animationTimer.Interval = TimeSpan.FromMilliseconds(newValue);
		}

		/// <summary>
		/// Gets or sets the refresh interval, in milliseconds, of the Spectrum Analyzer.
		/// </summary>
		/// <remarks>
		/// The valid range of the interval is 10 milliseconds to 1000 milliseconds.
		/// </remarks>
		[Category("Common")]
		public int RefreshInterval
		{
			// IMPORTANT: To maintain parity between setting a property in XAML and procedural code, do not touch the getter and setter inside this dependency property!
			get
			{
				return (int)GetValue(RefreshIntervalProperty);
			}
			set
			{
				SetValue(RefreshIntervalProperty, value);
			}
		}
		#endregion


		#region ThresholdWindow
		/// <summary>
		/// Identifies the <see cref="ThresholdWindow" /> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ThresholdWindowProperty = DependencyProperty.Register("ThresholdWindow", typeof(int), typeof(Spectrum), new UIPropertyMetadata(5, OnThresholdWindowChanged, OnCoerceThresholdWindow));

		private static object OnCoerceThresholdWindow(DependencyObject o, object value)
		{
			Spectrum spectrum = o as Spectrum;
			if (spectrum != null)
				return spectrum.OnCoerceThresholdWindow((int)value);
			else
				return value;
		}

		private static void OnThresholdWindowChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			Spectrum spectrum = o as Spectrum;
			if (spectrum != null)
				spectrum.OnThresholdWindowChanged((int)e.OldValue, (int)e.NewValue);
		}

		/// <summary>
		/// Coerces the value of <see cref="ThresholdWindow"/> when a new value is applied.
		/// </summary>
		/// <param name="value">The value that was set on <see cref="ThresholdWindow"/></param>
		/// <returns>The adjusted value of <see cref="ThresholdWindow"/></returns>
		protected virtual int OnCoerceThresholdWindow(int value)
		{
			value = Math.Max(value, 0);
			return value;
		}

		/// <summary>
		/// Called after the <see cref="ThresholdWindow"/> value has changed.
		/// </summary>
		/// <param name="oldValue">The previous value of <see cref="ThresholdWindow"/></param>
		/// <param name="newValue">The new value of <see cref="ThresholdWindow"/></param>
		protected virtual void OnThresholdWindowChanged(int oldValue, int newValue)
		{
			UpdateBarLayout();
		}

		/// <summary>
		/// Gets or sets the spacing between the bars.
		/// </summary>
		[Category("Common")]
		public int ThresholdWindow
		{
			// IMPORTANT: To maintain parity between setting a property in XAML and procedural code, do not touch the getter and setter inside this dependency property!
			get
			{
				return (int)GetValue(ThresholdWindowProperty);
			}
			set
			{
				SetValue(ThresholdWindowProperty, value);
			}
		}
		#endregion

		#region ThresholdThreshold
		/// <summary>
		/// Identifies the <see cref="ThresholdThreshold" /> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ThresholdThresholdProperty = DependencyProperty.Register("ThresholdThreshold", typeof(double), typeof(Spectrum), new UIPropertyMetadata(0.1, OnThresholdThresholdChanged, OnCoerceThresholdThreshold));

		private static object OnCoerceThresholdThreshold(DependencyObject o, object value)
		{
			Spectrum spectrum = o as Spectrum;
			if (spectrum != null)
				return spectrum.OnCoerceThresholdThreshold((double)value);
			else
				return value;
		}

		private static void OnThresholdThresholdChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			Spectrum spectrum = o as Spectrum;
			if (spectrum != null)
				spectrum.OnThresholdThresholdChanged((double)e.OldValue, (double)e.NewValue);
		}

		/// <summary>
		/// Coerces the value of <see cref="ThresholdThreshold"/> when a new value is applied.
		/// </summary>
		/// <param name="value">The value that was set on <see cref="ThresholdThreshold"/></param>
		/// <returns>The adjusted value of <see cref="ThresholdThreshold"/></returns>
		protected virtual double OnCoerceThresholdThreshold(double value)
		{
			value = Math.Max(value, 0);
			return value;
		}

		/// <summary>
		/// Called after the <see cref="ThresholdThreshold"/> value has changed.
		/// </summary>
		/// <param name="oldValue">The previous value of <see cref="ThresholdThreshold"/></param>
		/// <param name="newValue">The new value of <see cref="ThresholdThreshold"/></param>
		protected virtual void OnThresholdThresholdChanged(double oldValue, double newValue)
		{
			UpdateBarLayout();
		}

		/// <summary>
		/// Gets or sets the spacing between the bars.
		/// </summary>
		[Category("Common")]
		public double ThresholdThreshold
		{
			// IMPORTANT: To maintain parity between setting a property in XAML and procedural code, do not touch the getter and setter inside this dependency property!
			get
			{
				return (double)GetValue(ThresholdThresholdProperty);
			}
			set
			{
				SetValue(ThresholdThresholdProperty, value);
			}
		}
		#endregion

		#region ThresholdInfluence
		/// <summary>
		/// Identifies the <see cref="ThresholdInfluence" /> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ThresholdInfluenceProperty = DependencyProperty.Register("ThresholdInfluence", typeof(double), typeof(Spectrum), new UIPropertyMetadata(0.5, OnThresholdInfluenceChanged, OnCoerceThresholdInfluence));

		private static object OnCoerceThresholdInfluence(DependencyObject o, object value)
		{
			Spectrum spectrum = o as Spectrum;
			if (spectrum != null)
				return spectrum.OnCoerceThresholdInfluence((double)value);
			else
				return value;
		}

		private static void OnThresholdInfluenceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			Spectrum spectrum = o as Spectrum;
			if (spectrum != null)
				spectrum.OnThresholdInfluenceChanged((double)e.OldValue, (double)e.NewValue);
		}

		/// <summary>
		/// Coerces the value of <see cref="ThresholdInfluence"/> when a new value is applied.
		/// </summary>
		/// <param name="value">The value that was set on <see cref="ThresholdInfluence"/></param>
		/// <returns>The adjusted value of <see cref="ThresholdInfluence"/></returns>
		protected virtual double OnCoerceThresholdInfluence(double value)
		{
			value = Math.Max(value, 0);
			return value;
		}

		/// <summary>
		/// Called after the <see cref="ThresholdInfluence"/> value has changed.
		/// </summary>
		/// <param name="oldValue">The previous value of <see cref="ThresholdInfluence"/></param>
		/// <param name="newValue">The new value of <see cref="ThresholdInfluence"/></param>
		protected virtual void OnThresholdInfluenceChanged(double oldValue, double newValue)
		{
			UpdateBarLayout();
		}

		/// <summary>
		/// Gets or sets the spacing between the bars.
		/// </summary>
		[Category("Common")]
		public double ThresholdInfluence
		{
			// IMPORTANT: To maintain parity between setting a property in XAML and procedural code, do not touch the getter and setter inside this dependency property!
			get
			{
				return (double)GetValue(ThresholdInfluenceProperty);
			}
			set
			{
				SetValue(ThresholdInfluenceProperty, value);
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
				Interval = TimeSpan.FromMilliseconds(defaultUpdateInterval),
			};
			animationTimer.Tick += animationTimer_Tick;

			BassEngine.Instance.PropertyChanged += soundPlayer_PropertyChanged;
			UpdateBarLayout();
			animationTimer.Start();
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
			UpdateSpectrum();
		}
		#endregion

		#region Private Drawing Methods
		DispatcherTimer fpstimer;
		int fps = 0;
		int framesSinceLastFPSTick = 0;
		private void UpdateSpectrum()
		{
			if (fpstimer == null)
			{
				fpstimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle)
				{
					Interval = TimeSpan.FromMilliseconds(250),
				};
				fpstimer.Tick += fps_Tick;
				fpstimer.Start();
			}

			if (spectrumCanvas == null || spectrumCanvas.RenderSize.Width < 1 || spectrumCanvas.RenderSize.Height < 1)
				return;

			if (BassEngine.Instance.IsPlaying && !BassEngine.Instance.GetFFTData(channelData))
				return;

			UpdateSpectrumShapes();
		}

		private void fps_Tick(object sender, EventArgs e)
		{
			fps = framesSinceLastFPSTick * 4;
			framesSinceLastFPSTick = 0;
		}

		private List<HSLColor> PrevColors = new List<HSLColor>();
		private TextBlock Vals = new TextBlock
		{
			Background = new SolidColorBrush(Colors.Transparent),
			Foreground = new SolidColorBrush(Colors.White),
			VerticalAlignment = VerticalAlignment.Top,
			HorizontalAlignment = HorizontalAlignment.Stretch,
			FontSize = 20
		};
		private void UpdateSpectrumShapes()
		{
			framesSinceLastFPSTick++;

			bool allZero = true;
			double fftBucketHeight = 0f;
			double barHeight = 0f;
			double lastPeakHeight = 0f;
			double peakYPos = 0f;
			double height = spectrumCanvas.RenderSize.Height;
			int barIndex = 0;
			double peakDotHeight = Math.Max(barWidth / 2.0f, 1);
			double barHeightScale = (height - peakDotHeight);
			int barittercount = 0;
			List<Tuple<HSLColor, int>> HSL_Values = new List<Tuple<HSLColor, int>>();

			for (int i = minimumFrequencyIndex; i <= maximumFrequencyIndex; i++)
			{
				// If we're paused, keep drawing, but set the current height to 0 so the peaks fall.
				if (!BassEngine.Instance.IsPlaying)
				{
					barHeight = 0f;
				}
				else // Draw the maximum value for the bar's band
				{

					fftBucketHeight = (channelData[i] * scaleFactorLinear) * barHeightScale;

					if (barHeight < fftBucketHeight)
						barHeight = fftBucketHeight;
					if (barHeight < 0f)
						barHeight = 0f;
				}

				// If this is the last FFT bucket in the bar's group, draw the bar.
				int currentIndexMax = barIndexMax[barIndex];
				if (i == currentIndexMax)
				{
					//DO NOT REMOVE, EVERYTHING BREAKS?
					barittercount++;

					// Peaks can't surpass the height of the control.
					if (barHeight > height)
						barHeight = height;
					
					peakYPos = barHeight;

					double xCoord = (barWidth * barIndex) + 1;

					((Rectangle)spectrumCanvas.Children[barIndex]).Margin = new Thickness(xCoord, (height - 1) - barHeight, 0, 0);
					((Rectangle)spectrumCanvas.Children[barIndex]).Height = barHeight;


					//Color Calcs;
					HSLColor HSL = new HSLColor(((double)barittercount) / BarCount, 1, Math.Pow((barHeight) / height, 1.5));
					System.Drawing.Color RGB = HSL;


					((Rectangle)spectrumCanvas.Children[barIndex]).Fill = new SolidColorBrush(Color.FromRgb(RGB.R, RGB.G, RGB.B));
					HSL_Values.Add(new Tuple<HSLColor, int>(HSL, barittercount));


					lastPeakHeight = barHeight;
					barHeight = 0f;
					barIndex++;
				}
			}

			List<int> Peaks = ThresholdDetection(HSL_Values.Select(x => x.Item1.Luminosity).ToList());

			List<Tuple<HSLColor, int>> TopHSLValues = new List<Tuple<HSLColor, int>>();
			for (int i = 0; i < Peaks.Count; i++)
				if (Peaks[i] > 0)
					TopHSLValues.Add(HSL_Values[i]);


			//Show visually which bars colors are being averaged
			foreach (var rt in spectrumCanvas.Children)
				if (rt is Rectangle r)
					r.StrokeThickness = 0;
			for (int i = 0; i < TopHSLValues.Count(); i++)
			{
				((Rectangle)spectrumCanvas.Children[TopHSLValues[i].Item2 - 1]).Stroke = new SolidColorBrush(Colors.White);
				((Rectangle)spectrumCanvas.Children[TopHSLValues[i].Item2 - 1]).StrokeThickness = 2;
			}

			if (GetTemplateChild("PART_SpectrumCanvas") != null && GetTemplateChild("PART_SpectrumCanvas") is Canvas c)
			{
				if (c.Background == null)
					c.Background = new SolidColorBrush(Colors.Black);

				//get actual average of the n/4 values
				double AVGH = 0;
				for (int i = 0; i < TopHSLValues.Count(); i++)
					if (TopHSLValues[i].Item1.Luminosity > 0)
						AVGH = (AVGH * i + TopHSLValues[i].Item1.Hue) / (i + 1);

				//Get a good Value for the background color that is related to how "loud" the sound is by selecting the top 25%
				HSLColor temp = new HSLColor(AVGH, 1, TopHSLValues.Count > 0 ? TopHSLValues.OrderByDescending(x => x.Item1.Luminosity).Take((int)(Math.Round(TopHSLValues.Count * .1) > 0 ? Math.Round(TopHSLValues.Count * .1) : TopHSLValues.Count)).Average(x => x.Item1.Luminosity) : 0);
				if (double.IsNaN(temp.Luminosity))
					temp.Luminosity = 0;

				//add current color to end of list, and remove extras if needed
				PrevColors.Add(temp);
				if (PrevColors.Count() > BlendedFrames + 1)
					for (int i = PrevColors.Count(); i > BlendedFrames + 1; i--)
						PrevColors.RemoveRange(0, 1);

				//average last i background colors together
				HSLColor Background = new HSLColor(0, 1, 0);
				for (int i = 0; i < PrevColors.Count(); i++)
					Background = new HSLColor((Background.Hue * i + PrevColors[i].Hue) / (i + 1), 1, (Background.Luminosity * i + PrevColors[i].Luminosity) / (i + 1));

				//convert for solid brush
				Color AVG_RGB = Color.FromArgb(255, Background.ToColor().R, Background.ToColor().G, Background.ToColor().B);
				c.Background = new SolidColorBrush(Color.FromRgb((byte)AVG_RGB.R, (byte)AVG_RGB.G, (byte)AVG_RGB.B));


				Vals.Text = "\t\t\t" + Math.Round(Background.Hue, 4).ToString("0.000") + ", 1.000, " + Math.Round(Background.Luminosity, 4).ToString("0.000") + "\tFPS: " + fps;
			}

			if (allZero && !BassEngine.Instance.IsPlaying)
				animationTimer.Stop();
		}

		private double CalculatePercentile(double[] sequence, double excelPercentile)
		{
			Array.Sort(sequence);
			int N = sequence.Length;
			double n = (N - 1) * excelPercentile + 1;
			// Another method: double n = (N + 1) * excelPercentile;
			if (n == 1d) return sequence[0];
			else if (n == N) return sequence[N - 1];
			else
			{
				int k = (int)n;
				double d = n - k;
				return sequence[k - 1] + d * (sequence[k] - sequence[k - 1]);
			}
		}

		private List<int> ThresholdDetection(List<double> y)
		{
			try
			{
				//Add pad to front of y so that initial values can be picked up. Adding /these/ values to the front helps the algorithm build a better picture of the first peak (instead of selecting everything before the first peak)
				List<double> InvertedThreshold = new List<double>(y.GetRange(0, ThresholdWindow + 1));
				for (int i = 0; i <= ThresholdWindow; i++)
					y.Insert(0, y[i + i - (i == 0 ? 0 : 1)]);

				//set these up to the right size
				List<int> signals = new List<int>(Enumerable.Repeat(0, y.Count));
				List<double> FilteredY = new List<double>(Enumerable.Repeat(0.0, y.Count));
				List<double> avgFilter = new List<double>(Enumerable.Repeat(0.0, y.Count));
				List<double> stdFilter = new List<double>(Enumerable.Repeat(0.0, y.Count));

				//Copy the section of data over
				List<double> subList = new List<double>(Enumerable.Repeat(0.0, ThresholdWindow));
				for (int i = 0; i < ThresholdWindow; i++)
					subList[i] = y[i];

				//Get our first datapoint
				avgFilter[ThresholdWindow] = subList.Average();
				stdFilter[ThresholdWindow] = StandardDeviation(subList);

				for (int i = ThresholdWindow + 1; i < y.Count; i++)
				{
					if (Math.Abs(y[i] - avgFilter[i - 1]) > ThresholdThreshold * stdFilter[i - 1])
					{
						if (y[i] > avgFilter[i - 1])
							signals[i] = 1;
						else
							signals[i] = -1;
						FilteredY[i] = ThresholdInfluence * y[i] + (1 - ThresholdInfluence) * FilteredY[i - 1];
					}
					else
					{
						signals[i] = 0;
						FilteredY[i] = y[i];
					}

					//remake sublist
					for (int j = i - ThresholdWindow; j < i; j++)
						subList.Add(FilteredY[i]);

					avgFilter[i] = subList.Average();
					stdFilter[i] = StandardDeviation(subList);
				}

				//remove pad
				for (int i = 0; i <= ThresholdWindow; i++)
					signals.Remove(0);

				return signals;
			}
			catch
			{
				//if break, return empty list
				return new List<int>(Enumerable.Repeat(0, y.Count));
			}
		}

		/// <summary>
		/// Finds standard deviation of a list of doubles
		/// </summary>
		/// <param name="values">List to find deviation of</param>
		/// <returns></returns>
		private double StandardDeviation(List<double> values)
		{
			var g = values.Aggregate(new { mean = 0.0, sum = 0.0, count = 0 },
			(acc, val) =>
			{
				var newcount = acc.count + 1;
				double delta = val - acc.mean;
				var newmean = acc.mean + delta / newcount;
				return new { mean = newmean, sum = acc.sum + delta * (val - newmean), count = newcount };
			});
			return Math.Sqrt(g.sum / g.count);
		}

		private void UpdateBarLayout()
		{
			if (spectrumCanvas == null)
				return;

			barWidth = Math.Max(((double)(spectrumCanvas.RenderSize.Width) / (double)BarCount), 1);
			maximumFrequencyIndex = Math.Min(BassEngine.Instance.GetFFTFrequencyIndex(MaximumFrequency) + 1, 2047);
			minimumFrequencyIndex = Math.Min(BassEngine.Instance.GetFFTFrequencyIndex(MinimumFrequency), 2047);
			bandWidth = Math.Max(((double)(maximumFrequencyIndex - minimumFrequencyIndex)) / spectrumCanvas.RenderSize.Width, 1.0);

			int actualBarCount;
			if (barWidth >= 1.0d)
				actualBarCount = BarCount;
			else
				actualBarCount = Math.Max((int)(spectrumCanvas.RenderSize.Width / barWidth), 1);

			int indexCount = maximumFrequencyIndex - minimumFrequencyIndex;
			int linearIndexBucketSize = (int)Math.Round((double)indexCount / (double)actualBarCount, 0);
			List<int> maxIndexList = new List<int>();
			List<int> maxLogScaleIndexList = new List<int>();
			double maxLog = Math.Log(actualBarCount, actualBarCount);
			for (int i = 1; i < actualBarCount; i++)
			{
				maxIndexList.Add(minimumFrequencyIndex + (i * linearIndexBucketSize));
				int logIndex = (int)((maxLog - Math.Log((actualBarCount + 1) - i, (actualBarCount + 1))) * indexCount) + minimumFrequencyIndex;
				maxLogScaleIndexList.Add(logIndex);
			}
			maxIndexList.Add(maximumFrequencyIndex);
			maxLogScaleIndexList.Add(maximumFrequencyIndex);
			barIndexMax = maxIndexList.ToArray();
			barLogScaleIndexMax = maxLogScaleIndexList.ToArray();

			barHeights = new double[actualBarCount];

			spectrumCanvas.Children.Clear();

			double height = spectrumCanvas.RenderSize.Height;
			for (int i = 0; i < actualBarCount; i++)
			{
				double xCoord = (barWidth * i) + 1;
				Rectangle barRectangle = new Rectangle()
				{
					Margin = new Thickness(xCoord, height, 0, 0),
					Width = barWidth,
					Height = 0,
					Style = BarStyle,
					Visibility = BarVisibility
				};
				spectrumCanvas.Children.Add(barRectangle);
			}


			spectrumCanvas.Children.Add(Vals);

			ActualBarWidth = barWidth;
		}
		#endregion

		#region Event Handlers
		private void soundPlayer_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "IsPlaying":
					if (BassEngine.Instance.IsPlaying && !animationTimer.IsEnabled)
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