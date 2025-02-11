using System.Diagnostics;
using System.IO;

using MameLauncher.Models;
using MameLauncher.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Wpf.Ui.Abstractions.Controls;

namespace MameLauncher.Pages;

/// <summary>
/// Interaction logic for GameListPage.xaml
/// </summary>
public partial class GameListPage : INavigableView<GamesViewModel>
{
	public GamesViewModel ViewModel { get; }

	public GameListPage()
	{
		ViewModel = new GamesViewModel(this);
		DataContext = this;

		InitializeComponent();

		MainWindow main = (MainWindow)System.Windows.Application.Current.MainWindow;
		main.TitleBar.Title = $"MAME {App.MameBuild}";
		App.IsInited = true;
	}

	public void OnNavigatedTo()
	{
		ListCtrl1.SelectedIndex = ViewModel.FindMameItemIndex(App.SelectedMameItems[0]);
		ListCtrl1.ScrollIntoView(ListCtrl1.SelectedItem);
		SetPreviewImage((Models.MameItem)ListCtrl1.SelectedItem);
	}

	private static void SetPreviewImage(Models.MameItem? item)
	{
		if (App.IsInited) {
			MainWindow main = (MainWindow)System.Windows.Application.Current.MainWindow;
			main.SetPreviewImage(0, item != null ? item.ShortName : String.Empty);
		}
	}

	private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		var item = (Models.MameItem)ListCtrl1.SelectedItem;
		SetPreviewImage(item);
	}

	private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		var item = (Models.MameItem)((Wpf.Ui.Controls.ListViewItem)sender).Content;
		Debug.WriteLine($"MouseDoubleClick: {item.ShortName}");
		MainWindow main = (MainWindow)System.Windows.Application.Current.MainWindow;
		main.RunMame(item.ShortName);
	}
}
