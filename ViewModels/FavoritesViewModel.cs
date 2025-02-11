using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using Microsoft.Data.Sqlite;
using MameLauncher.Models;
using System.Diagnostics;
using System.IO;
using MameLauncher.Pages;

namespace MameLauncher.ViewModels;

public partial class FavoritesViewModel : ViewModel
{
	public FavoritesViewModel(FavoritesPage _view)
	{
		View = _view;
		_favoritesItems = LoadFavoritesItems();
	}

	private FavoritesPage View { get; }

	[ObservableProperty]
	private ObservableCollection<MameItem>? _favoritesItems;

	private Dictionary<String, int>? mameNameDict = null;

	public int FindMameItemIndex(String? shortName)
	{
		if (mameNameDict != null && shortName != null && mameNameDict.TryGetValue(shortName, out int value))
			return value;
		return -1;
	}

	private ObservableCollection<MameItem> LoadFavoritesItems()
	{
		var items = new ObservableCollection<MameItem> {};
		//var favorites = new List<MameItem>();
		mameNameDict = [];
		var ini_path = Path.Combine(App.MameDir!, "ui\\favorites.ini");
		if (!File.Exists(ini_path)) { return items; }
		using var reader = new StreamReader(ini_path);
		{
			while (!reader.EndOfStream) {
				var line = reader.ReadLine();
				if (!String.IsNullOrEmpty(line) && line[0] != '[') {
					var _name = line;
					var _desc = reader.ReadLine() ?? String.Empty;
					for (int i = 0; i < 14; i++)
						reader.ReadLine();
					items.Add(new MameItem(_name, "", "", _desc, "", ""));
					while (!reader.EndOfStream) {
						_name = reader.ReadLine() ?? String.Empty;
						_desc = reader.ReadLine() ?? String.Empty;
						for (int i = 0; i < 14; i++)
							reader.ReadLine();
						mameNameDict.Add(_name, items.Count);
						items.Add(new MameItem(_name, "", "", _desc, "", ""));
					}
					break;
				}
			}
		}
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
