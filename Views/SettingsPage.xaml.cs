using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MameLauncher.ViewModels;
using Wpf.Ui.Abstractions.Controls;

namespace MameLauncher.Pages;

/// <summary>
/// Interaction logic for SettingsPage.xaml
/// </summary>
public partial class SettingsPage : INavigableView<SettingsViewModel>
{
	public SettingsViewModel ViewModel { get; }

	public SettingsPage()
	{
		ViewModel = new SettingsViewModel(this);
		DataContext = this;

		InitializeComponent();
	}
	public void OnNavigatedTo()
	{
		if (App.IsInited) {
			MainWindow main = (MainWindow)System.Windows.Application.Current.MainWindow;
			main.SetPreviewImage(-1, String.Empty);
		}
	}
}
