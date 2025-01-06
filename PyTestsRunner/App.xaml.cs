using ControlzEx.Theming;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Threading;

namespace PyTestsRunner
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public App()
		{
			
			this.DispatcherUnhandledException += App_DispatcherUnhandledException;


		}

		

		public static void ChangeDarkLight(bool isLightTheme)
		{
			if (Current == null)
				return;

			if (isLightTheme)
				ThemeManager.Current.ChangeTheme(Current, "Light.Cobalt");
			else
				ThemeManager.Current.ChangeTheme(Current, "Dark.Cobalt");
		}



		private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			MessageBox.Show("Un-handled exception caught\r\n\r\n" + e, "Error");
			e.Handled = true;
		}
	}

}
