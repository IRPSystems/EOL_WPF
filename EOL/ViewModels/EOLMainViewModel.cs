
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EOL.Models;
using Services.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using System.ComponentModel;
using System.Reflection;
using DeviceHandler.Models;
using DeviceCommunicators.Models;
using DeviceHandler.Models.DeviceFullDataModels;
using Entities.Enums;
using DeviceCommunicators.Services;
using System.Linq;
using DeviceHandler.ViewModels;
using System.Windows;
using EOL.Views;
using EOL.Services;
using EOL_Tester.Classes;
using DeviceHandler.Views;

namespace EOL.ViewModels
{
	public class EOLMainViewModel : ObservableObject
	{
		public class ModeType
		{
			public string Name { get; set; }

			public override string ToString()
			{
				return Name;
			}
		}


		#region Properties

		public string Version { get; set; }

		public DevicesContainer DevicesContainter { get; set; }


		public OperatorViewModel OperatorVM { get; set; }

		public CommunicationViewModel CommunicationSettings { get; set; }

		public SettingsViewModel SettingsVM { get; set; }

		public List<ModeType> ModeTypeList { get; set; }

		public string SelectedMode { get; set; }

		#endregion Properties

		#region Fields

		private EOLSettings _eolSettings;

		private SettingsData _settingsData;

		private UserDefaultSettings _userDefaultSettings;

		private ReadDevicesFileService _readDevicesFile;

		private SetupSelectionViewModel _setupSelectionVM;

		#endregion Fields

		#region Constructor

		public EOLMainViewModel()
		{
			_settingsData = new SettingsData();

			UserConfigManager userConfigManager = new UserConfigManager();

			_userDefaultSettings = userConfigManager.ReadConfig();

			SelectedMode = "Operator";

			Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

			ChangeDarkLightCommand = new RelayCommand(ChangeDarkLight);

			ClosingCommand = new RelayCommand<CancelEventArgs>(Closing);
			LoadedCommand = new RelayCommand(Loaded);
			ModesDropDownMenuItemCommand = new RelayCommand<string>(ModesDropDownMenuItem);

			

			CommunicationSettingsCommand = new RelayCommand(InitCommunicationSettings);
			SettingsCommand = new RelayCommand(Settings);

			ModeTypeList = new List<ModeType>
				{
					new ModeType() { Name = "Operator" },
					new ModeType() { Name = "Admin" },
				};
		}

		#endregion Constructor

		#region Methods

		private void Closing(CancelEventArgs e)
		{
			EOLSettings.SaveEvvaUserData("EOL", _eolSettings);
		}

		#region Load

		private void Loaded()
		{
			try
			{

				LoggerService.Init("EOL.log", Serilog.Events.LogEventLevel.Information);
				LoggerService.Inforamtion(this, "-------------------------------------- EOL ---------------------");


				LoggerService.Inforamtion(this, "Starting Loaded of EOLMainViewModel");

				_eolSettings = EOLSettings.LoadEvvaUserData("EOL");
				ChangeDarkLight();


				_readDevicesFile = new ReadDevicesFileService();
				_setupSelectionVM =
					new SetupSelectionViewModel(_eolSettings.DeviceSetupUserData, _readDevicesFile, false);
				SetupSelectionWindowView setupSelectionView = new SetupSelectionWindowView();
				setupSelectionView.SetDataContext(_setupSelectionVM);
				bool? resutl = setupSelectionView.ShowDialog();
				if (resutl != true)
				{
					Closing(null);
					Application.Current.Shutdown();
					return;
				}

				DevicesContainter = new DevicesContainer();
				DevicesContainter.DevicesFullDataList = new ObservableCollection<DeviceFullData>();
				DevicesContainter.DevicesList = new ObservableCollection<DeviceData>();
				DevicesContainter.TypeToDevicesFullData = new Dictionary<DeviceTypesEnum, DeviceFullData>();
				UpdateSetup();


				CommunicationSettings = new CommunicationViewModel(DevicesContainter);

				_settingsData.FilesList = new ObservableCollection<FilesData>()
				{
					new FilesData() { Description = "Reports Path" },
					new FilesData() { Description = "Main Script Path" },
					new FilesData() { Description = "Sub Script Path" },
					new FilesData() { Description = "Monitor Script Path" },
                    new FilesData() { Description = "Abort Script Path" },
                    new FilesData() { Description = "First Flash File Path" },
					new FilesData() { Description = "Second Flash File Path" },
				};
				

				SettingsVM = new SettingsViewModel(_settingsData, _userDefaultSettings);

				OperatorVM = new OperatorViewModel(
					DevicesContainter, 
					_eolSettings.ScriptUserData,
					_userDefaultSettings, SettingsVM);

				try
				{
					foreach (DeviceFullData deviceFullData in DevicesContainter.DevicesFullDataList)
					{
						deviceFullData.InitCheckConnection();
					}
				}
				catch (Exception ex)
				{
					LoggerService.Error(this, "Failed to init the communication check", ex);

				}
			}
			catch (Exception ex)
			{
				LoggerService.Error(this, "Failed to init the main window", "Startup Error", ex);
			}
		}

		private void UpdateSetup()
		{

			ObservableCollection<DeviceData> deviceList = _setupSelectionVM.DevicesList;


			List<DeviceData> newDevices = new List<DeviceData>();
			foreach (DeviceData deviceData in deviceList)
			{
				DeviceData existingDevice =
					DevicesContainter.DevicesList.ToList().Find((d) => d.DeviceType == deviceData.DeviceType);
				if (existingDevice == null)
					newDevices.Add(deviceData);
			}

			List<DeviceData> removedDevices = new List<DeviceData>();
			foreach (DeviceData deviceData in DevicesContainter.DevicesList)
			{
				DeviceData existingDevice =
					deviceList.ToList().Find((d) => d.DeviceType == deviceData.DeviceType);
				if (existingDevice == null)
					removedDevices.Add(deviceData);
			}




			foreach (DeviceData device in removedDevices)
			{
				DeviceFullData deviceFullData =
					DevicesContainter.DevicesFullDataList.ToList().Find((d) => d.Device.DeviceType == device.DeviceType);
				deviceFullData.Disconnect();

				DevicesContainter.DevicesFullDataList.Remove(deviceFullData);
				DevicesContainter.DevicesList.Remove(deviceFullData.Device);
				DevicesContainter.TypeToDevicesFullData.Remove(deviceFullData.Device.DeviceType);
			}



			foreach (DeviceData device in newDevices)
			{
				DeviceFullData deviceFullData = DeviceFullData.Factory(device);

				deviceFullData.Init("EOL");

				DevicesContainter.DevicesFullDataList.Add(deviceFullData);
				DevicesContainter.DevicesList.Add(device as DeviceData);
				if (DevicesContainter.TypeToDevicesFullData.ContainsKey(device.DeviceType) == false)
					DevicesContainter.TypeToDevicesFullData.Add(device.DeviceType, deviceFullData);

				deviceFullData.Connect();
			}
		}

		#endregion Load



		private void ChangeDarkLight()
		{
			_eolSettings.IsLightTheme = !_eolSettings.IsLightTheme;
			App.ChangeDarkLight(_eolSettings.IsLightTheme);
		}

		private void InitCommunicationSettings()
		{
			CommunicationWindowView communicationWindowView = new CommunicationWindowView()
			{
				DataContext = CommunicationSettings,
				Owner = Application.Current.MainWindow
			};

			communicationWindowView.Show();
		}

		private void Settings()
		{
			SettingsView settingsView = new SettingsView()
			{
				DataContext = SettingsVM,
				Owner = Application.Current.MainWindow
			};


			settingsView.Show();
		}

		private void ModesDropDownMenuItem(string mode)
		{
			switch (mode)
			{
				case "Admin":
					SelectedMode = "Admin";
					OperatorVM.Run.IsAdminMode = true;
					break;
				case "Operator":
					SelectedMode = "Operator";
					OperatorVM.Run.IsAdminMode = false;
					break;
			}
		}

		#endregion Methods

		#region Commands

		public RelayCommand ChangeDarkLightCommand { get; private set; }

		public RelayCommand LoadedCommand { get; private set; }
		public RelayCommand<CancelEventArgs> ClosingCommand { get; private set; }

		public RelayCommand CommunicationSettingsCommand { get; private set; }
		public RelayCommand SettingsCommand { get; private set; }

		public RelayCommand<string> ModesDropDownMenuItemCommand { get; private set; }

		#endregion Commands
	}
}
