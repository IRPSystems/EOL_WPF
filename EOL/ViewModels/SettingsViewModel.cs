
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceHandler.ViewModels;
using EOL.Models;
using EOL.Services;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using FlowDirection = System.Windows.FlowDirection;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using TestersDB_Lib;
using Microsoft.Data.SqlClient;
namespace EOL.ViewModels
{
    public class SettingsViewModel: ObservableObject
    {
		#region Properties

		public SettingsData SettingsData { get; set; }
		public double DescriptsionColumnWidth { get; set; }

		public SetupSelectionViewModel SetupSelectionVM { get; set; }
        public SettingsAdminViewModel SettingsAdminVM { get; set; }
        public DatabaseHandler DataBaseHandlerVM;
		public bool IsAdminMode { get; set; }

        public SqlConnectionStringBuilder builder { get; set; }

        // Define connection variables
        private string _server = "irp-systems-sqlserver.database.windows.net";
        private string _database = "Testers_DB"; // Replace with your actual database name if different
        private string _userId = "TestersDB_Manager";
        private string _password = "DontPanic123"; // **Warning:** Avoid hardcoding passwords in production

        private string _DBconnectionString;
        public string DBconnectionString
        {
            get => _DBconnectionString;
            set
            {
                _DBconnectionString = value;
                OnPropertyChanged();
            }
        }
		#endregion Properties

		#region Fields

		UserDefaultSettings _userDefaultSettings;
        UserConfigManager _userConfigManager;

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
        public event Action DBconnectionEvent; 

		#endregion Events



		#region Constructor

		public SettingsViewModel(
            EOLSettings eolSettings,
            UserConfigManager userConfigManager,
			SetupSelectionViewModel setupSelectionVM,
            DatabaseHandler DBhandler)
        {
			SettingsData = eolSettings.GeneralData;
            _userDefaultSettings = eolSettings.UserDefaultSettings;
            _userConfigManager = userConfigManager;
            SetupSelectionVM = setupSelectionVM;
            DataBaseHandlerVM = DBhandler;

            DataBaseHandlerVM.DBConnectionLog += WritetoTerminal;
            DataBaseHandlerVM.init();
            SettingsData.DataBaseConnection = DataBaseHandlerVM._connectionString;

            BrowseFilePathCommand = new RelayCommand<FilesData>(BrowseFilePath);
			LoadedCommand = new RelayCommand(Loaded);
            ClosingCommand = new RelayCommand<CancelEventArgs>(Closing);
            ConnectionCommand = new RelayCommand(DBConnect);
            ClearTerminalCommand = new RelayCommand(ClearTerminal);

            SettingsAdminVM = new SettingsAdminViewModel(eolSettings);
		}

        #endregion Constructor

        #region Methods

        private void Closing(CancelEventArgs e)
        {
            _userDefaultSettings.DatabaseConnectionURL = DataBaseHandlerVM._connectionString;
            _userDefaultSettings.EOLRackSN = SettingsData.RackNumber;
            _userConfigManager.SaveConfig(_userDefaultSettings);

            SetupSelectionVM.CloseOKCommand.Execute(null);
			SettingsWindowClosedEvent?.Invoke();

		}

        private void DBConnect()
        {
            DataBaseHandlerVM.Connect();
        }

        private void WritetoTerminal(string text)
        {
            DBconnectionString += text + "\r\n\r\n";
        }
        private void ClearTerminal()
        {
            DBconnectionString = string.Empty;
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
                    ReportsPathEventChanged?.Invoke();
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
                else if (fileData.Description == "Safety Officer Script Path")
                {
                    fileData.Path = _userDefaultSettings.DefaultSafetyScriptFile;
                    SafetyScriptEventChanged?.Invoke();
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
                    FirstFlashFileEventChanged?.Invoke();
                }
                else
                {
                    fileData.Path = _userDefaultSettings.SecondFlashFilePath;
                    SecondFlashFileEventChanged?.Invoke();
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
                    FirstFlashFileEventChanged.Invoke();
                }
                else
                {
                    _userDefaultSettings.SecondFlashFilePath = fileData.Path;
                    SecondFlashFileEventChanged.Invoke();
                }
            }
        }

		#endregion Methods

		#region Commands

		public RelayCommand<FilesData> BrowseFilePathCommand { get; private set; }
		public RelayCommand LoadedCommand { get; private set; }
        public RelayCommand ConnectionCommand { get; private set; }
        public RelayCommand ClearTerminalCommand { get; private set; }

        public RelayCommand<CancelEventArgs> ClosingCommand { get; private set; }
		

		#endregion Commands

	}
}
