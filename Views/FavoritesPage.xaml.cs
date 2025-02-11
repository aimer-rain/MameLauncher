using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.IO;

using MameLauncher.Models;
using MameLauncher.ViewModels;
using Wpf.Ui.Abstractions.Controls;

namespace MameLauncher.Pages;

/// <summary>
/// Interaction logic for FavoritesPage.xaml
/// </summary>
public partial class FavoritesPage : INavigableView<FavoritesViewModel>
{
	public FavoritesViewModel ViewModel { get; }

	public FavoritesPage()
    {
		ViewModel = new FavoritesViewModel(this);
		DataContext = this;

		InitializeComponent();

		ListCtrl2.ScrollIntoView(ListCtrl2.SelectedItem);
	}

	public void OnNavigatedTo()
	{
		ListCtrl2.SelectedIndex = ViewModel.FindMameItemIndex(App.SelectedMameItems[1]);
		ListCtrl2.ScrollIntoView(ListCtrl2.SelectedItem);
		SetPreviewImage((Models.MameItem)ListCtrl2.SelectedItem);
	}

	private static void SetPreviewImage(Models.MameItem? item)
	{
		if (App.IsInited) {
			MainWindow main = (MainWindow)System.Windows.Application.Current.MainWindow;
			main.SetPreviewImage(1, item != null ? item.ShortName : String.Empty);
		}
	}

	private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		var item = (Models.MameItem)ListCtrl2.SelectedItem;
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
