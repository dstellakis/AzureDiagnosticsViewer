/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocatorTemplate xmlns:vm="clr-namespace:Meteo.ViewModel"
                                   x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
*/

using System;
using GalaSoft.MvvmLight;

namespace AzureDiagnosticsViewer.ViewModels
{
	public class ViewModelLocator
	{
		static ViewModelLocator()
		{
			Microsoft.Practices.ServiceLocation.ServiceLocator.SetLocatorProvider(() => GalaSoft.MvvmLight.Ioc.SimpleIoc.Default);

			GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.Register<MainViewModel>();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This non-static member is needed for data binding purposes.")]
		public MainViewModel MainViewModel
		{
			get
			{
				//return SearchViewModelStatic;
				return Microsoft.Practices.ServiceLocation.ServiceLocator.Current.GetInstance<MainViewModel>();
			}
		}

		/// <summary>
		/// Cleans up all the resources.
		/// </summary>
		public static void Cleanup()
		{
		}
	}
}