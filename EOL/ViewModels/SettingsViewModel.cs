
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EOL.Models;
using EOL_Tester.Classes;
using Microsoft.Win32;
using ScriptHandler.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EOL.ViewModels
{
    public class SettingsViewModel: ObservableObject
    {
		#region Properties

		public SettingsData SettingsData { get; set; }
		public double DescriptsionColumnWidth { get; set; }

		UserDefaultSettings _userDefaultSettings;

        public event Action MainScriptEventChanged;
        public event Action MonitorScriptEventChanged;
        public event Action ProjectScriptEventChanged;

        #endregion Properties

        #region Constructor

        public SettingsViewModel(SettingsData settingsData, UserDefaultSettings userDefaultSettings)
        {
			SettingsData = settingsData;
            _userDefaultSettings = userDefaultSettings;
            BrowseFilePathCommand = new RelayCommand<FilesData>(BrowseFilePath);
			LoadedCommand = new RelayCommand(Loaded);
			
		}

		#endregion Constructor

		#region Methods

		private void Loaded()
		{
			GetDescriptsionColumnWidth();
		}

		private void GetDescriptsionColumnWidth()
		{
			double maxWidth = 0;
			foreach (FilesData filesData in SettingsData.FilesList)
			{
				if (filesData == null || string.IsNullOrEmpty(filesData.Description))
					continue;

				TextBlock textBlock = new TextBlock();
				var formattedText = new FormattedText(
					filesData.Description,
					CultureInfo.CurrentCulture,
					FlowDirection.LeftToRight,
					new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch),
					14,
					Brushes.Black,
					new NumberSubstitution(),
					VisualTreeHelper.GetDpi(textBlock).PixelsPerDip);

				if (formattedText.Width > maxWidth)
					maxWidth = formattedText.Width;
			}

			DescriptsionColumnWidth = maxWidth + 10;
			if(DescriptsionColumnWidth > 250)
				DescriptsionColumnWidth = 250;
		}


		private void BrowseFilePath(FilesData filesData)
        {
			OpenFileDialog openFileDialog = new OpenFileDialog();			
			bool? result = openFileDialog.ShowDialog();
			if (result != true)
				return;

			filesData.Path = openFileDialog.FileName;

			if (filesData.Description == "Reports Path")
			{
				_userDefaultSettings.ReportsSavingPath = filesData.Path;
			}
			else if (filesData.Description == "Main Script Path")
			{
				_userDefaultSettings.DefaultMainSeqConfigFile = filesData.Path;
				MainScriptEventChanged.Invoke();
            }
			else if (filesData.Description == "Project Script Path")
			{
				_userDefaultSettings.DefaultSubscriptFile = filesData.Path;
				ProjectScriptEventChanged.Invoke();
            }
			else if (filesData.Description == "Monitor Script Path")
			{
				_userDefaultSettings.DefaultMonitorLogScript = filesData.Path;
				MonitorScriptEventChanged.Invoke();
            }
			else if (filesData.Description == "First Flash File Path")
			{
				_userDefaultSettings.FirstFlashFilePath = filesData.Path;
			}
			else
			{
				_userDefaultSettings.SecondFlashFilePath = filesData.Path;
			}
        }

		#endregion Methods

		#region Commands

		public RelayCommand<FilesData> BrowseFilePathCommand { get; private set; }
		public RelayCommand LoadedCommand { get; private set; }

		#endregion Commands

	}
}
