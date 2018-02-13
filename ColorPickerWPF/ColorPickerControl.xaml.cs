using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Drawing;
using ColorPickerWPF.Code;
using MColor = System.Windows.Media.Color;
using UserControl = System.Windows.Controls.UserControl;
using System.ComponentModel;
using System.IO;

namespace ColorPickerWPF
{
	/// <summary>
	/// Interaction logic for ColorPickerControl.xaml
	/// </summary>
	public partial class ColorPickerControl : UserControl
	{

		#region Color
		/// <summary>
		/// Identifies the <see cref="Color" /> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(Color), typeof(ColorPickerControl), new UIPropertyMetadata(Color.White, OnColorChanged, OnCoerceColor));

		private static object OnCoerceColor(DependencyObject o, object value)
		{
			ColorPickerControl ColorPickerControl = o as ColorPickerControl;
			if (ColorPickerControl != null)
				return ColorPickerControl.OnCoerceColor((Color)value);
			else
				return value;
		}

		private static void OnColorChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			ColorPickerControl ColorPickerControl = o as ColorPickerControl;
			if (ColorPickerControl != null)
				ColorPickerControl.OnColorChanged((Color)e.OldValue, (Color)e.NewValue);
		}

		/// <summary>
		/// Coerces the value of <see cref="Color"/> when a new value is applied.
		/// </summary>
		/// <param name="value">The value that was set on <see cref="Color"/></param>
		/// <returns>The adjusted value of <see cref="Color"/></returns>
		protected virtual Color OnCoerceColor(Color value)
		{
			return value;
		}

		/// <summary>
		/// Called after the <see cref="Color"/> value has changed.
		/// </summary>
		/// <param name="oldValue">The previous value of <see cref="Color"/></param>
		/// <param name="newValue">The new value of <see cref="Color"/></param>
		protected virtual void OnColorChanged(Color oldValue, Color newValue)
		{
			SetColor(newValue);
		}

		/// <summary>
		/// Gets or sets the number of bars to show on the sprectrum analyzer.
		/// </summary>
		/// <remarks>A bar's width can be a minimum of 1 pixel. If the BarSpacing and Color property result
		/// in the bars being wider than the chart itself, the Color will automatically scale down.</remarks>
		[Category("Common")]
		public Color Color
		{
			// IMPORTANT: To maColorain parity between setting a property in XAML and procedural code, do not touch the getter and setter inside this dependency property!
			get
			{
				return (Color)GetValue(ColorProperty);
			}
			set
			{
				SetValue(ColorProperty, value);
			}
		}
		#endregion

		public delegate void ColorPickerChangeHandler(Color color);

		public event ColorPickerChangeHandler OnPickColor;

		public bool IsSettingValues = false;

		protected const int NumColorsFirstSwatch = 39;
		protected const int NumColorsSecondSwatch = 112;



		public ColorPickerControl()
		{
			InitializeComponent();
			this.DataContext = this;

			RSlider.Slider.Maximum = 255;
			GSlider.Slider.Maximum = 255;
			BSlider.Slider.Maximum = 255;
			HSlider.Slider.Maximum = 360;
			SSlider.Slider.Maximum = 1;
			LSlider.Slider.Maximum = 1;


			RSlider.Label.Content = "R";
			RSlider.Slider.TickFrequency = 1;
			RSlider.Slider.IsSnapToTickEnabled = true;
			GSlider.Label.Content = "G";
			GSlider.Slider.TickFrequency = 1;
			GSlider.Slider.IsSnapToTickEnabled = true;
			BSlider.Label.Content = "B";
			BSlider.Slider.TickFrequency = 1;
			BSlider.Slider.IsSnapToTickEnabled = true;

			HSlider.Label.Content = "H";
			HSlider.Slider.TickFrequency = 1;
			HSlider.Slider.IsSnapToTickEnabled = true;
			SSlider.Label.Content = "S";
			//SSlider.Slider.TickFrequency = 1;
			//SSlider.Slider.IsSnapToTickEnabled = true;
			LSlider.Label.Content = "V";
			//LSlider.Slider.TickFrequency = 1;
			//LSlider.Slider.IsSnapToTickEnabled = true;


			SetColor(Color);

		}


		public void SetColor(Color color)
		{
			Color = color;

			IsSettingValues = true;

			RSlider.Slider.Value = Color.R;
			RSlider.Label.Content = "Red";
			GSlider.Slider.Value = Color.G;
			GSlider.Label.Content = "Green";
			BSlider.Slider.Value = Color.B;
			BSlider.Label.Content = "Blue";

			SSlider.Slider.Value = Color.GetSaturation();
			SSlider.Label.Content = "Saturation";
			LSlider.Slider.Value = Color.GetBrightness();
			LSlider.Label.Content = "Brightness";
			HSlider.Slider.Value = Color.GetHue();
			HSlider.Label.Content = "Hue";
			ColorDisplayBorder.Background = new System.Windows.Media.SolidColorBrush(MColor.FromRgb(Color.R, Color.G, Color.B));

			IsSettingValues = false;
			OnPickColor?.Invoke(color);

		}


		protected void SampleImageClick(System.Windows.Point pos)
		{
			Bitmap bitmap;
			using (MemoryStream outStream = new MemoryStream())
			{
				BitmapEncoder enc = new BmpBitmapEncoder();

				enc.Frames.Add(BitmapFrame.Create(SampleImage.Source as BitmapSource));
				enc.Save(outStream);
				bitmap = new Bitmap(outStream);
			}
			double XScale = SampleImage.ActualWidth / bitmap.Width;
			double YScale = SampleImage.ActualHeight / bitmap.Height;
			var pixel = bitmap.GetPixel((int)(pos.X / XScale), (int)(pos.Y / YScale));
			SetColor(Color.FromArgb(pixel.R, pixel.G, pixel.B));
		}


		private void SampleImage_OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			var pos = e.GetPosition(SampleImage);
			var img = SampleImage.Source as BitmapSource;
			SampleImageClick(pos);
		}

		private void Swatch_OnOnPickColor(Color color)
		{
			SetColor(color);
		}

		private void HSlider_OnOnValueChanged(double value)
		{
			if (!IsSettingValues)
			{
				var s = Color.GetSaturation();
				var l = Color.GetBrightness();
				var h = (float)value;
				var a = 255;
				Color = Util.FromAhsb(a, h, s, l);

				SetColor(Color);
			}
		}




		private void RSlider_OnOnValueChanged(double value)
		{
			if (!IsSettingValues)
			{
				Color = Color.FromArgb((byte)value, Color.B, Color.G);
				SetColor(Color);
			}
		}

		private void GSlider_OnOnValueChanged(double value)
		{
			if (!IsSettingValues)
			{
				Color = Color.FromArgb(Color.R, Color.B, (byte)value);
				SetColor(Color);
			}
		}

		private void BSlider_OnOnValueChanged(double value)
		{
			if (!IsSettingValues)
			{
				Color = Color.FromArgb(Color.R, (byte)value, Color.G);
				SetColor(Color);
			}
		}

		private void SSlider_OnOnValueChanged(double value)
		{
			if (!IsSettingValues)
			{
				var s = (float)value;
				var l = Color.GetBrightness();
				var h = Color.GetHue();
				var a = 255;
				Color = Util.FromAhsb(a, h, s, l);

				SetColor(Color);
			}

		}

		private void LSlider_OnOnValueChanged(double value)
		{
			if (!IsSettingValues)
			{
				var s = Color.GetSaturation();
				var l = (float)value;
				var h = Color.GetHue();
				var a = 255;
				Color = Util.FromAhsb(a, h, s, l);

				SetColor(Color);
			}
		}

		private void SampleImage_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
				SampleImageClick(e.GetPosition(sender as IInputElement));

		}
	}
}
