
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceHandler.Models;
using DeviceHandler.ViewModels;
using EOL.Models;
using EOL.Services;
using Microsoft.IdentityModel.Tokens;
using ScriptHandler.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
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

		public SetupSelectionViewModel SetupSelectionVM { get; set; }
        public SettingsAdminViewModel SettingsAdminVM { get; set; }
        public CANBusSettingsListViewModel CANBusSettingsVM { get; set; }

        public bool IsAdminMode { get; set; }

        public List<string> PSoCPortsList { get; set; }

		#endregion Properties

		#region Fields

		UserDefaultSettings _userDefaultSettings;
        UserConfigManager _userConfigManager;

        private FlashingHandler _flashingHandler;

        #endregion Fields

        #region Events
        public event Action ReportsPathEventChanged;
        public event Action MainScriptEventChanged;
        public event Action MonitorScriptEventChanged;
        public event Action SubScriptEventChanged;
        public event Action SafetyScriptEventChanged;
        public event Action AbortScriptEventChanged;
        public event Action FirstFlashFileEventChanged;
        public event Action SecondFlashFileEventChanged;
        public event Action SettingsWindowClosedEvent;

		#endregion Events



		#region Constructor

		public SettingsViewModel(
            EOLSettings eolSettings,
            UserConfigManager userConfigManager,
            FlashingHandler flashingHandler,
			SetupSelectionViewModel setupSelectionVM,
            WatsConnectionMonitor watsConnection,
            DevicesContainer devicesContainer)
        {
			SettingsData = eolSettings.GeneralData;
            _userDefaultSettings = eolSettings.UserDefaultSettings;
            _userConfigManager = userConfigManager;
            SetupSelectionVM = setupSelectionVM;
            _flashingHandler = flashingHandler;
            CANBusSettingsVM = new CANBusSettingsListViewModel(
                devicesContainer,
                setupSelectionVM);
            BrowseFilePathCommand = new RelayCommand<FilesData>(BrowseFilePath);
			LoadedCommand = new RelayCommand(Loaded);
            ClosingCommand = new RelayCommand<CancelEventArgs>(Closing);
            PSoCPort1_SelectionChangedCommand = new RelayCommand(PSoCPort1_SelectionChanged);
			PSoCPort2_SelectionChangedCommand = new RelayCommand(PSoCPort2_SelectionChanged);
            PSoCPort1_DropDownOpenedCommand = new RelayCommand(PSoCPort1_DropDownOpened);
			PSoCPort2_DropDownOpenedCommand = new RelayCommand(PSoCPort2_DropDownOpened);
			SettingsAdminVM = new SettingsAdminViewModel(eolSettings , watsConnection);
            MainScriptEventChanged += SettingsAdminVM.OnMainScriptChanged;
		}

        #endregion Constructor

        #region Methods

        private void Closing(CancelEventArgs e)
        {
            _userDefaultSettings.EOLRackSN = SettingsData.RackNumber;
            _userDefaultSettings.WatsTestCode = SettingsAdminVM.SelectedTestOperation?.Code.ToString() ?? string.Empty;
            //if (CANBusSettingsVM.CANBusDeviceList != null && !CANBusSettingsVM.CANBusDeviceList.IsNullOrEmpty())
            //{
            //    foreach (var canBusDevice in CANBusSettingsVM.CANBusDeviceList[0].CANBusDevice.DeviceDataList)
            //    {
            //        if (!_userDefaultSettings.CANBus.Any(x => x.DeviceType == canBusDevice.DeviceType))
            //        {
            //            CANBusDeviceConfig canBusDeviceconfig = new CANBusDeviceConfig
            //            {
            //                DeviceType = canBusDevice.DeviceType,
            //                NodeId = canBusDevice.NodeId
            //            };
            //            _userDefaultSettings.CANBus.Add(canBusDeviceconfig);
            //        }
            //    }
            //}
            _userConfigManager.SaveConfig(_userDefaultSettings);

            SetupSelectionVM.CloseOKCommand.Execute(null);
			SettingsWindowClosedEvent?.Invoke();

		}

        private void Loaded()
		{
			GetDescriptsionColumnWidth();
            //string errorMsg = string.Empty;
            //PSoCPortsList = _flashingHandler.GetPSOCDeviceList(out errorMsg);
        }



        public void LoadUserConfigToSettingsView()
        {
            string fileNotFoundPrefixMsg = "File not found, please select a valid path:";
            string completeErrMsg = fileNotFoundPrefixMsg;
            foreach (FilesData fileData in SettingsData.FilesList)
            {
                if (fileData.Description == "Reports Path")
                {
                    if (_userDefaultSettings.ReportsSavingPath == null)
                    {
                        continue;
                    }
                    if (!Directory.Exists(_userDefaultSettings.ReportsSavingPath)) // check if the path is valid
                    {
                        completeErrMsg += "\r\n" + fileData.Description;
                    }
                    else
                    {
                        fileData.Path = _userDefaultSettings.ReportsSavingPath;
                        ReportsPathEventChanged?.Invoke();
                    }
                }
                else if (fileData.Description == "Main Script Path")
                {
                    if (_userDefaultSettings.DefaultMainSeqConfigFile == null)
                    {
                        continue;
                    }
                    if (!File.Exists(_userDefaultSettings.DefaultMainSeqConfigFile)) // check if the path is valid
                    {
                        completeErrMsg += "\r\n" + fileData.Description;
                    }
                    else
                    {
                        fileData.Path = _userDefaultSettings.DefaultMainSeqConfigFile;
                        MainScriptEventChanged?.Invoke();
                    }
                }
                else if (fileData.Description == "Sub Script Path")
                {
                    if (String.IsNullOrEmpty(_userDefaultSettings.DefaultSubScriptFile))
                    {
                        continue;
                    }
                    if (!File.Exists(_userDefaultSettings.DefaultSubScriptFile)) // check if the path is valid
                    {
                        completeErrMsg += "\r\n" + fileData.Description;
                    }
                    else
                    {
                        fileData.Path = _userDefaultSettings.DefaultSubScriptFile;
                        SubScriptEventChanged?.Invoke();
                    }
                }
                else if (fileData.Description == "Safety Officer Script Path")
                {
                    if (String.IsNullOrEmpty( _userDefaultSettings.DefaultSafetyScriptFile))
                    {
                        continue;
                    }
                    if (!File.Exists(_userDefaultSettings.DefaultSafetyScriptFile)) // check if the path is valid
                    {
                        completeErrMsg += "\r\n" + fileData.Description;
                    }
                    else
                    {
                        fileData.Path = _userDefaultSettings.DefaultSafetyScriptFile;
                        SafetyScriptEventChanged?.Invoke();
                    }
                }
                else if (fileData.Description == "Abort Script Path")
                {
                    if (String.IsNullOrEmpty(_userDefaultSettings.DefaultAbortScriptFile))
                    {
                        continue;
                    }
                    if (!File.Exists(_userDefaultSettings.DefaultAbortScriptFile)) // check if the path is valid
                    {
                        completeErrMsg += "\r\n" + fileData.Description;
                    }
                    else
                    {
                        fileData.Path = _userDefaultSettings.DefaultAbortScriptFile;
                        AbortScriptEventChanged?.Invoke();
                    }
                }
                else if (fileData.Description == "Monitor Script Path")
                {
                    if (String.IsNullOrEmpty(_userDefaultSettings.DefaultMonitorLogScript))
                    {
                        continue;
                    }
                    if (!File.Exists(_userDefaultSettings.DefaultMonitorLogScript)) // check if the path is valid
                    {
                        completeErrMsg += "\r\n" + fileData.Description;
                    }
                    else
                    {
                        fileData.Path = _userDefaultSettings.DefaultMonitorLogScript;
                        MonitorScriptEventChanged?.Invoke();
                    }
                }
                else if (fileData.Description == "First Flash File Path")
                {
                    if (String.IsNullOrEmpty(_userDefaultSettings.FirstFlashFilePath) )
                    {
                        continue;
                    }
                    if (!File.Exists(_userDefaultSettings.FirstFlashFilePath)) // check if the path is valid
                    {
                        completeErrMsg += "\r\n" + fileData.Description;
                    }
                    else
                    {
                        fileData.Path = _userDefaultSettings.FirstFlashFilePath;
                        FirstFlashFileEventChanged?.Invoke();
                    }
                }
                else
                {
                    if (String.IsNullOrEmpty(_userDefaultSettings.SecondFlashFilePath))
                    {
                        continue;
                    }
                    if (!File.Exists(_userDefaultSettings.SecondFlashFilePath)) // check if the path is valid
                    {
                        completeErrMsg += "\r\n" + fileData.Description;
                    }
                    else
                    {
                        fileData.Path = _userDefaultSettings.SecondFlashFilePath;
                        SecondFlashFileEventChanged?.Invoke();
                    }
                }
            }

            SettingsData.IsIgnorFail = _userDefaultSettings.isSofIgnore;
            SettingsData.IsPrintLabel = _userDefaultSettings.isPrintLabel;

            if (completeErrMsg != fileNotFoundPrefixMsg)
            {
                MessageBox.Show(completeErrMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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


		private void BrowseFilePath(FilesData fileData)
        {   
            if (fileData.Description == "Reports Path")
			{
                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                DialogResult result = folderBrowserDialog.ShowDialog();
                if (result != DialogResult.OK)
                    return;
                fileData.Path = folderBrowserDialog.SelectedPath;
                _userDefaultSettings.ReportsSavingPath = fileData.Path;
                ReportsPathEventChanged?.Invoke();
            }
			else
			{
                OpenFileDialog openFileDialog = new OpenFileDialog();
                bool? result = openFileDialog.ShowDialog();
                if (result != true)
                    return;

                fileData.Path = openFileDialog.FileName;

                if (fileData.Description == "Main Script Path")
                {
                    _userDefaultSettings.DefaultMainSeqConfigFile = fileData.Path;
                    MainScriptEventChanged?.Invoke();
                }
                else if (fileData.Description == "Sub Script Path")
                {
                    _userDefaultSettings.DefaultSubScriptFile = fileData.Path;
                    SubScriptEventChanged?.Invoke();
                }
                else if (fileData.Description == "Safety Officer Script Path")
                {
                    _userDefaultSettings.DefaultSafetyScriptFile = fileData.Path;
                    SafetyScriptEventChanged?.Invoke();
                }
                else if (fileData.Description == "Abort Script Path")
                {
                    _userDefaultSettings.DefaultAbortScriptFile = fileData.Path;
                    AbortScriptEventChanged?.Invoke();
                }
                else if (fileData.Description == "Monitor Script Path")
                {
                    _userDefaultSettings.DefaultMonitorLogScript = fileData.Path;
                    MonitorScriptEventChanged?.Invoke();
                }
                else if (fileData.Description == "First Flash File Path")
                { 
                    _userDefaultSettings.FirstFlashFilePath = fileData.Path;
                    FirstFlashFileEventChanged?.Invoke();
                }
                else
                {
                    _userDefaultSettings.SecondFlashFilePath = fileData.Path;
                    SecondFlashFileEventChanged?.Invoke();
                }
            }
        }

        private void PSoCPort1_SelectionChanged()
        {
            _userDefaultSettings.PSoC_Port1 = SettingsData.PSoCPort1;
        }

        private void PSoCPort2_SelectionChanged()
		{
            _userDefaultSettings.PSoC_Port2 = SettingsData.PSoCPort2;

        }

        private void PSoCPort1_DropDownOpened()
        {
            string errorMsg = string.Empty;
            PSoCPortsList = _flashingHandler.GetPSOCDeviceList(out errorMsg);
        }

        private void PSoCPort2_DropDownOpened()
		{
            string errorMsg = string.Empty;
            PSoCPortsList = _flashingHandler.GetPSOCDeviceList(out errorMsg);
        }

        #endregion Methods

        #region Commands

        public RelayCommand<FilesData> BrowseFilePathCommand { get; private set; }
		public RelayCommand LoadedCommand { get; private set; }
        public RelayCommand<CancelEventArgs> ClosingCommand { get; private set; }

		public RelayCommand PSoCPort1_SelectionChangedCommand { get; private set; }
		public RelayCommand PSoCPort2_SelectionChangedCommand { get; private set; }

		public RelayCommand PSoCPort1_DropDownOpenedCommand { get; private set; }
		public RelayCommand PSoCPort2_DropDownOpenedCommand { get; private set; }


		#endregion Commands

	}
}
