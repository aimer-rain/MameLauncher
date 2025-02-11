using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Wpf.Ui.Appearance;

namespace MameLauncher
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public App()
		{
		}

		protected override void OnExit(ExitEventArgs e)
		{
			if (System.IO.File.Exists(App.DbPath)) {
				using var connection = new SqliteConnection($"Data Source={App.DbPath}");
				try {
					connection.Open();
					var items = String.Join(',', SelectedMameItems);
					var cmd = connection.CreateCommand();
					cmd.CommandText = $"UPDATE mame SET items = '{items}' WHERE ROWID = 1;";
					cmd.ExecuteNonQuery();
				} catch (Exception ex) {
					Debug.WriteLine($"Processing failed: {ex.Message}");
				}
			}

			base.OnExit(e);
		}

		public static String? MameDir { get; set; }
		public static String? DbPath { get; set; }
		public static String? MameBuild { get; set; }

		public static bool IsInited { get; set; } = false;

		const int MAX_MAME_PAGES = 2;
		public static String[] SelectedMameItems = new String[MAX_MAME_PAGES];
	}

	internal sealed class ThemeToIndexConverter : IValueConverter
	{
		public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value is ApplicationTheme.Dark) {
				return 1;
			}

			if (value is ApplicationTheme.HighContrast) {
				return 2;
			}

			return 0;
		}

		public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value is 1) {
				return ApplicationTheme.Dark;
			}

			if (value is 2) {
				return ApplicationTheme.HighContrast;
			}

			return ApplicationTheme.Light;
		}
	}
}