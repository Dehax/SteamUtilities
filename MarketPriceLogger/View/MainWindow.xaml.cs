using MarketPriceLogger.ViewModel;
using System;
using System.Windows;

namespace MarketPriceLogger.View
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			Closed += MainWindow_Closed;
		}

		private void MainWindow_Closed(object sender, EventArgs e)
		{
			(DataContext as MainViewModel).StopAllThreads();
		}
	}
}
