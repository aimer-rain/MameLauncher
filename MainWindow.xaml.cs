using System.Windows.Media.Imaging;
using System.Diagnostics;
using Microsoft.Data.Sqlite;

using MameLauncher.Pages;
using Microsoft.Win32;
using System.Xml;
using System.Runtime.InteropServices;


namespace Win32
{
	public class Api
	{
		[DllImport("user32.dll")]
		public static extern IntPtr FindWindow(string lpClassName, string? lpWindowName);
		[DllImport("user32.dll")]
		public static extern IntPtr FindWindowEx(IntPtr hWnd1, IntPtr hWnd2, string lpsz1, string lpsz2);
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SetForegroundWindow(IntPtr hWnd);
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
	}
}

namespace MameLauncher
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
    {
		public MainWindow()
        {
			DataContext = this;

			Wpf.Ui.Appearance.SystemThemeWatcher.Watch(this);

			InitializeComponent();

			string baseDir = AppDomain.CurrentDomain.BaseDirectory;
			Debug.WriteLine("Base Directory: " + baseDir);
			App.DbPath = System.IO.Path.Combine(baseDir, path2: System.IO.Path.GetFileNameWithoutExtension(Environment.ProcessPath)!);
			App.DbPath += ".db";
			Debug.WriteLine($"DB Path: {App.DbPath}");
			var connectionString = $"Data Source={App.DbPath}";

			Debug.Assert(System.IO.File.Exists("sqlite3.dll"));
			SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_sqlite3());

			String? mame_build = null;
			bool bExistDbFile = System.IO.File.Exists(App.DbPath);
			if (!bExistDbFile) {
				var folderDialog = new OpenFolderDialog
				{
					Title = "Select MAME Folder",
					InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
				};

				if (folderDialog.ShowDialog() == true)
					App.MameDir = folderDialog.FolderName;
			} else {
				using var connection = new SqliteConnection(connectionString);
				try {
					connection.Open();
					var cmd = connection.CreateCommand();
					cmd.CommandText = "SELECT build, root, items FROM mame LIMIT 1;";
					{
						using var mame = cmd.ExecuteReader();
						if (mame.Read()) {
							mame_build = mame.GetString(0);
							App.MameDir = mame.GetString(1);
							var items = mame.GetString(2);
							if (!String.IsNullOrEmpty(items)) {
								App.SelectedMameItems = items.Split(',');
							}
						}
					}
					cmd.Dispose();
				} catch (Exception e) {
					Debug.WriteLine($"Processing failed: {e.Message}");
				}
			}

			if (String.IsNullOrEmpty(App.MameDir)) {
				if (bExistDbFile) {
					SqliteConnection.ClearAllPools();
					System.IO.File.Delete(App.DbPath);
				}
				App.Current.Shutdown();
				return;
			}

			var xmlPath = System.IO.Path.Combine(App.MameDir, "list.xml");
			App.MameBuild = GetMameBuildNum(xmlPath);
			if (String.IsNullOrEmpty(mame_build) || App.MameBuild != mame_build) {
				if (bExistDbFile) {
					SqliteConnection.ClearAllPools();
					System.IO.File.Delete(App.DbPath);
				}
				var itemCount = 0;
				using var connection = new SqliteConnection(connectionString);
				try {
					connection.Open();
					CreateTable(connection);
					var cmd = connection.CreateCommand();
					cmd.CommandText = $"INSERT INTO mame (build, root) VALUES ('{App.MameBuild}', '{App.MameDir}');";
					cmd.ExecuteNonQuery();

					var settings = new XmlReaderSettings
					{
						DtdProcessing = DtdProcessing.Ignore,
						ValidationType = ValidationType.DTD
					};
					using var xml = XmlReader.Create(xmlPath, settings);
					xml.ReadToFollowing("mame");
					cmd.Transaction = connection.BeginTransaction();
					while (xml.ReadToFollowing("machine")) {
						var shortName = xml.GetAttribute("name");
						if (shortName == null)
							continue;
						var srcFile = xml.GetAttribute("sourcefile") ?? string.Empty;
						var romOf = xml.GetAttribute("romof") ?? string.Empty;
						xml.ReadToFollowing("description");
						var desc = xml.ReadElementContentAsString().Replace('\"', '\'');
						if (xml.ReadToFollowing("year")) {
							var year = xml.ReadElementContentAsString();
							xml.ReadToFollowing("manufacturer");
							var manufact = xml.ReadElementContentAsString().Replace('\"', '\'');
							cmd.CommandText = "INSERT INTO machine (name, src, parent, desc, year, manufact) VALUES " +
								$"(\"{shortName}\", \"{srcFile}\", \"{romOf}\", \"{desc}\", \"{year}\", \"{manufact}\");";
							cmd.ExecuteNonQuery();
							itemCount++;
						}
					}
					cmd.Transaction.Commit();
					cmd.Dispose();
				} catch (Exception e) {
					Debug.WriteLine($"Processing failed: {e.Message}");
				}
			}

			Loaded += (_, _) => RootNavigation.Navigate(typeof(GameListPage));
		}

		public static string GetMameBuildNum(String xmlPath)
		{
			var settings = new XmlReaderSettings
			{
				DtdProcessing = DtdProcessing.Ignore,
				ValidationType = ValidationType.DTD
			};
			using var reader = XmlReader.Create(xmlPath, settings);
			reader.ReadToFollowing("mame");
			var build = reader.GetAttribute("build") ?? String.Empty;
			var pos = build.IndexOf(' ');
			if (pos == -1)
				return build;
			return build[..pos];
		}

		public static void CreateTable(SqliteConnection connection)
		{
			var cmd = connection.CreateCommand();
			cmd.CommandText = @"CREATE TABLE IF NOT EXISTS mame (build TEXT NOT NULL, root TEXT, items TEXT);";
			cmd.ExecuteNonQuery();
			cmd.CommandText = @"CREATE TABLE IF NOT EXISTS machine (
			name TEXT NOT NULL PRIMARY KEY, src TEXT, parent TEXT, desc TEXT NOT NULL, year TEXT, manufact TEXT);";
			cmd.ExecuteNonQuery();
			cmd.CommandText = @"CREATE UNIQUE INDEX IF NOT EXISTS idx_machine ON machine(name, desc, year, manufact);";
			cmd.ExecuteNonQuery();
		}

		public static void DeleteTable(SqliteConnection connection)
		{
			var cmd = connection.CreateCommand();
			cmd.CommandText = @"DELETE FROM mame; DELETE FROM machine;";
			cmd.ExecuteNonQuery();
		}

		public void SetPreviewImage(int idx, String mameName)
		{
			var imgPath = System.IO.Path.Combine(App.MameDir!, $"artpreview\\{mameName}.png");
			if (System.IO.File.Exists(imgPath)) {
				var uriSource = new Uri(imgPath, UriKind.Absolute);
				PreviewImg.Source = new BitmapImage(uriSource);
			} else {
				var uriSource = new Uri("pack://application:,,,/Assets/No-Image.png");
				PreviewImg.Source = new BitmapImage(uriSource);
			}
			if (idx >= 0)
				App.SelectedMameItems[idx] = mameName;
		}

		private bool _isMameRunning;
		public bool IsMameRunning { get { return _isMameRunning; } }

		private String? currentMameName = null;

		public bool RunMame(string mameName)
		{
			if (IsMameRunning) return false;
			currentMameName = mameName;
			Thread mameThread = new(start: MameProc!);
			mameThread.Start(this);
			return true;
		}

		private static void MameProc(object obj)
		{
			MainWindow main = (MainWindow)obj;
			var mameName = main.currentMameName;
			main._isMameRunning = true;
			ProcessStartInfo startInfo = new()
			{
				FileName = System.IO.Path.Combine(App.MameDir!, "mame.exe"),
				Arguments = $"-autosave -skip_gameinfo -video d3d -nowindow {mameName}",
				WorkingDirectory = App.MameDir!,
				UseShellExecute = false,
				CreateNoWindow = true,
			};
			var process = Process.Start(startInfo);
			if (process != null) {
				Thread.Sleep(500);
				IntPtr hwnd = Win32.Api.FindWindow("MAME", null);
				if (hwnd != IntPtr.Zero) {
					Debug.WriteLine($"hwnd: {hwnd}");
					Win32.Api.SetForegroundWindow(hwnd);
				}
				process.WaitForExit();
			}
			main._isMameRunning = false;
		}
	}
}
