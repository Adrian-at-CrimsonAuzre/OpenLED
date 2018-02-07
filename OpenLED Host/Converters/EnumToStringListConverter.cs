using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Factory_Logistics.Converters
{
	class EnumToStringListConverter : IValueConverter
	{
		/// <summary>
		/// Generic Enum to List<string> converter
		/// </summary>
		/// <param name="value">First value of Enum, as sent by a Control</param>
		/// <returns></returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			List<string> output = new List<string>();
			foreach (var enumchild in Enum.GetValues(value.GetType()))
				output.Add(enumchild.ToString());

			return output;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
