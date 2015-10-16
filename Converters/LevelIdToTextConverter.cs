using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace AzureDiagnosticsViewer.Converters
{
	public class LevelIdToTextConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			int level = int.Parse(value.ToString());

			if (level == 0)
				return "Undefined";
			else if (level == 1)
				return "Critical";
			else if (level == 2)
				return "Error";
			else if (level == 3)
				return "Warning";
			else if (level == 4)
				return "Information";
			else if (level == 5)
				return "Verbose";
			else
				return String.Empty;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
