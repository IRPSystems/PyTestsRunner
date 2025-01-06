using MahApps.Metro.Controls;
using PyTestsRunner.ViewModels;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PyTestsRunner.Views
{
	/// <summary>
	/// Interaction logic for PyTestsRunnerMainWindow.xaml
	/// </summary>
	public partial class PyTestsRunnerMainWindow : MetroWindow
	{
		public PyTestsRunnerMainWindow()
		{
			InitializeComponent();

			DataContext = new PyTestsRunnerMainViewModel();
		}

		private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			tbOutput.ScrollToEnd();
		}
    }
}