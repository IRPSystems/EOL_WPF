
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EOL.Models;
using EOL.Services;
using Microsoft.Win32;
using ScriptHandler.Interfaces;
using System;
using System.CodeDom;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using FlowDirection = System.Windows.FlowDirection;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace EOL.ViewModels
{
    public class SettingsViewModel: ObservableObject
    {
		#region Properties

		public SettingsData SettingsData { get; set; }
		public double DescriptsionColumnWidth { get; set; }

		UserDefaultSettings _userDefaultSettings;
        UserConfigManager _userConfigManager;

        public event Action MainScriptEventChanged;
        public event Action MonitorScriptEventChanged;
        public event Action SubScriptEventChanged;
        public event Action AbortScriptEventChanged;

        #endregion Properties

        #region Constructor

        public SettingsViewModel(SettingsData settingsData, UserDefaultSettings userDefaultSettings, UserConfigManager userConfigManager)
        {
			SettingsData = settingsData;
            _userDefaultSettings = userDefaultSettings;
            _userConfigManager = userConfigManager;
            BrowseFilePathCommand = new RelayCommand<FilesData>(BrowseFilePath);
			LoadedCommand = new RelayCommand(Loaded);
            ClosingCommand = new RelayCommand<CancelEventArgs>(Closing);

            //LoadUserConfigToSettingsView();
        }

        #endregion Constructor

        #region Methods

        private void Closing(CancelEventArgs e)
        {
            _userConfigManager.SaveConfig(_userDefaultSettings);
        }

        private void Loaded()
		{
			GetDescriptsionColumnWidth();
		}

        public void LoadUserConfigToSettingsView()
        {
            foreach(FilesData fileData in SettingsData.FilesList)
            {
                if (fileData.Description == "Reports Path")
                {
                    fileData.Path = _userDefaultSettings.ReportsSavingPath;
                    MainScriptEventChanged?.Invoke();
                }
                else if (fileData.Description == "Main Script Path")
                {
                    fileData.Path = _userDefaultSettings.DefaultMainSeqConfigFile;
                    MainScriptEventChanged?.Invoke();
                }
                else if (fileData.Description == "Sub Script Path")
                {
                    fileData.Path = _userDefaultSettings.DefaultSubScriptFile;
                    SubScriptEventChanged?.Invoke();
                }
                else if (fileData.Description == "Abort Script Path")
                {
                    fileData.Path = _userDefaultSettings.DefaultAbortScriptFile;
                    AbortScriptEventChanged?.Invoke();
                }
                else if (fileData.Description == "Monitor Script Path")
                {
                    fileData.Path = _userDefaultSettings.DefaultMonitorLogScript;
                    MonitorScriptEventChanged?.Invoke();
                }
                else if (fileData.Description == "First Flash File Path")
                {
                    fileData.Path = _userDefaultSettings.FirstFlashFilePath;
                }
                else
                {
                    fileData.Path = _userDefaultSettings.SecondFlashFilePath;
                }
            }
            SettingsData.IsIgnorFail = _userDefaultSettings.isSofIgnore;
            SettingsData.IsPrintLabel = _userDefaultSettings.isPrintLabel;
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
            if (filesData.Description == "Reports Path")
			{
                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                DialogResult result = folderBrowserDialog.ShowDialog();
                if (result != DialogResult.OK)
                    return;
                filesData.Path = folderBrowserDialog.SelectedPath;
                _userDefaultSettings.ReportsSavingPath = filesData.Path;
            }
			else
			{
                OpenFileDialog openFileDialog = new OpenFileDialog();
                bool? result = openFileDialog.ShowDialog();
                if (result != true)
                    return;

                filesData.Path = openFileDialog.FileName;

                if (filesData.Description == "Main Script Path")
                {
                    _userDefaultSettings.DefaultMainSeqConfigFile = filesData.Path;
                    MainScriptEventChanged.Invoke();
                }
                else if (filesData.Description == "Sub Script Path")
                {
                    _userDefaultSettings.DefaultSubScriptFile = filesData.Path;
                    SubScriptEventChanged.Invoke();
                }
                else if (filesData.Description == "Abort Script Path")
                {
                    _userDefaultSettings.DefaultAbortScriptFile = filesData.Path;
                    AbortScriptEventChanged.Invoke();
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
        }

		#endregion Methods

		#region Commands

		public RelayCommand<FilesData> BrowseFilePathCommand { get; private set; }
		public RelayCommand LoadedCommand { get; private set; }
        public RelayCommand<CancelEventArgs> ClosingCommand { get; private set; }

        #endregion Commands

    }
}
