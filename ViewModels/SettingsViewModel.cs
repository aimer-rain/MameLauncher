using CommunityToolkit.Mvvm.ComponentModel;
using MameLauncher.Pages;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace MameLauncher.ViewModels;

public partial class SettingsViewModel : ViewModel
{
	public SettingsViewModel(SettingsPage _view)
	{
		View = _view;
	}

	private SettingsPage View { get; }

	private bool _isInitialized = false;

	[ObservableProperty]
	private string _appVersion = string.Empty;

	[ObservableProperty]
	private ApplicationTheme _currentApplicationTheme = ApplicationTheme.Unknown;

	public override void OnNavigatedTo()
	{
		if (!_isInitialized) {
			InitializeViewModel();
		}

		View.OnNavigatedTo();
	}

	partial void OnCurrentApplicationThemeChanged(ApplicationTheme oldValue, ApplicationTheme newValue)
	{
		ApplicationThemeManager.Apply(newValue);
	}

	private void InitializeViewModel()
	{
		CurrentApplicationTheme = ApplicationThemeManager.GetAppTheme();
		AppVersion = $"{GetAssemblyVersion()}";

		//ApplicationThemeManager.Changed += OnThemeChanged;

		_isInitialized = true;
	}

	private static string GetAssemblyVersion()
	{
		return Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? string.Empty;
	}
}
