using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using Microsoft.Data.Sqlite;
using MameLauncher.Models;
using System.Diagnostics;
using System.IO;
using MameLauncher.Pages;

namespace MameLauncher.ViewModels;

public partial class GamesViewModel : ViewModel
{
	public GamesViewModel(GameListPage _view)
	{
		View = _view;
		availableItems = LoadMameItems();
		_basicListViewItems = availableItems;
	}

	private GameListPage View { get; }

	[ObservableProperty]
	private ObservableCollection<MameItem>? _basicListViewItems;

	private readonly ObservableCollection<MameItem> availableItems;

	private Dictionary<String, int>? mameNameDict = null;

	public int FindMameItemIndex(String? shortName)
	{
		if (mameNameDict != null && shortName != null && mameNameDict.TryGetValue(shortName, out int value))
			return value;
		return -1;
	}

	private ObservableCollection<MameItem> LoadMameItems()
	{
		var items = new ObservableCollection<MameItem> {};
		mameNameDict = [];
		var roms_dir = Path.Combine(App.MameDir!, "roms");
		var clock = Stopwatch.StartNew();
		var connectionString = $"Data Source={App.DbPath}";
		using var connection = new SqliteConnection(connectionString);
		try {
			connection.Open();
			var cmd = connection.CreateCommand();
			cmd.CommandText = "SELECT * FROM machine ORDER BY desc;";
			using var reader = cmd.ExecuteReader();
			while (reader.Read()) {
				var rom_path = Path.Combine(roms_dir, reader.GetString(0) + ".zip");
				if (File.Exists(rom_path)) {
					var item = new MameItem(reader);
					items.Add(item);
					//item.PrintInfo();
					//Console.WriteLine("-------------------------");
					mameNameDict.Add(item.ShortName, items.Count - 1);
				}
			}
		} catch (Exception) {
		}
		Debug.WriteLine($"LoadMameItems() Elapsed: {clock.Elapsed.TotalMilliseconds}ms");
		MainWindow main = (MainWindow)System.Windows.Application.Current.MainWindow;
		if (items.Count > 0)
			main.NavItemAvailable.Content = $"Available ({items.Count})";
		else
			main.NavItemAvailable.Content = "Available";
		return items;
	}

	public override void OnNavigatedTo()
	{
		View.OnNavigatedTo();
	}

	public override void OnNavigatedFrom()
	{
	}
}
