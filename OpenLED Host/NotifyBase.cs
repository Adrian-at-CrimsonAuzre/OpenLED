using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OpenLED_Host
{
	/// <summary>
	/// Impliments INotifyPropertyChanged
	/// </summary>
	public class NotifyBase : INotifyPropertyChanged
	{
		//base for INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;
		public virtual void NotifyPropertyChanged([CallerMemberName]string member = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(member));
		}
	}
}
