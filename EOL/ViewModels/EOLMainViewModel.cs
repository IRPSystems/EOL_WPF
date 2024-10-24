﻿
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
using DeviceHandler.Views;
using Syncfusion.DocIO.DLS;

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

		//private UserDefaultSettings _eolSettings.UserDefaultSettings;

		private ReadDevicesFileService _readDevicesFile;

		private SetupSelectionViewModel _setupSelectionVM;

		private UserConfigManager _userConfigManager;

		private RunData _runData;

		#endregion Fields

		#region Constructor

		public EOLMainViewModel()
		{

			_userConfigManager = new UserConfigManager();

			_eolSettings = EOLSettings.LoadEOLUserData("EOL");

			_runData = new RunData();

			_userConfigManager.ReadConfig(_eolSettings);

			LoadConfigToUI();

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

        private void LoadConfigToUI()
        {
            _runData.OperatorName = _eolSettings.UserDefaultSettings.OperatorName;
			_runData.PartNumber = _eolSettings.UserDefaultSettings.PartNumber;
        }

        #endregion Constructor

        #region Methods

        private void Closing(CancelEventArgs e)
		{
			EOLSettings.SaveEOLUserData("EOL", _eolSettings);
			_userConfigManager.SaveConfig(_eolSettings.UserDefaultSettings);
		}

		#region Load

		private void Loaded()
		{
			try
			{

				LoggerService.Init("EOL.log", Serilog.Events.LogEventLevel.Information);
				LoggerService.Inforamtion(this, "-------------------------------------- EOL ---------------------");


				LoggerService.Inforamtion(this, "Starting Loaded of EOLMainViewModel");

				
				ChangeDarkLight();


				_readDevicesFile = new ReadDevicesFileService();
				_setupSelectionVM =
					new SetupSelectionViewModel(_eolSettings.DeviceSetupUserData, _readDevicesFile);
				InitSetupView();

				if (_eolSettings.DeviceSetupUserData.SetupDevicesList == null ||
					_eolSettings.DeviceSetupUserData.SetupDevicesList.Count == 0)
				{
					SetupSelectionWindowView setupSelectionView = new SetupSelectionWindowView();
					setupSelectionView.SetDataContext(_setupSelectionVM);
					bool? resutl = setupSelectionView.ShowDialog();
					if (resutl != true)
					{
						Closing(null);
						Application.Current.Shutdown();
						return;
					}
				}

				DevicesContainter = new DevicesContainer();
				DevicesContainter.DevicesFullDataList = new ObservableCollection<DeviceFullData>();
				DevicesContainter.DevicesList = new ObservableCollection<DeviceData>();
				DevicesContainter.TypeToDevicesFullData = new Dictionary<DeviceTypesEnum, DeviceFullData>();
				UpdateSetup();


				CommunicationSettings = new CommunicationViewModel(DevicesContainter);

				if (_eolSettings.GeneralData == null)
				{
					_eolSettings.GeneralData = new SettingsData()
					{
						FilesList = new ObservableCollection<FilesData>()
						{
							new FilesData() { Description = "Reports Path" },
							new FilesData() { Description = "Main Script Path" },
							new FilesData() { Description = "Sub Script Path" },
							new FilesData() { Description = "Monitor Script Path" },
                            new FilesData() { Description = "Safety Officer Script Path" },
                            new FilesData() { Description = "Abort Script Path" },
							new FilesData() { Description = "First Flash File Path" },
							new FilesData() { Description = "Second Flash File Path" },
						}
					};
				}

				SettingsVM = new SettingsViewModel(
					_eolSettings.GeneralData, 
					_eolSettings.UserDefaultSettings, 
					_userConfigManager,
					_setupSelectionVM);
				SettingsVM.SettingsWindowClosedEvent += SettingsVM_SettingsWindowClosedEvent;

				OperatorVM = new OperatorViewModel(
					DevicesContainter, 
					_eolSettings.ScriptUserData,
					_eolSettings.UserDefaultSettings, SettingsVM, _runData);

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

		private void InitSetupView()
		{
			InitSetupView_single(_setupSelectionVM.DevicesList);
			InitSetupView_single(_setupSelectionVM.DevicesSourceList);
		}

		private void InitSetupView_single(ObservableCollection<DeviceData> devicesList)
		{
			List<DeviceData> devicesToRemoveList = new List<DeviceData>();
			foreach (DeviceData device in devicesList)
			{
				if (device.DeviceType != DeviceTypesEnum.MCU &&
					device.DeviceType != DeviceTypesEnum.MCU_B2B &&
					device.DeviceType != DeviceTypesEnum.ZimmerPowerMeter &&
					device.DeviceType != DeviceTypesEnum.NI_6002 &&
					device.DeviceType != DeviceTypesEnum.NI_6002_2 &&
					device.DeviceType != DeviceTypesEnum.Printer_TSC &&
					device.DeviceType != DeviceTypesEnum.NumatoGPIO)
				{
					devicesToRemoveList.Add(device);
					continue;
				}

				if (device.DeviceType == DeviceTypesEnum.MCU && _eolSettings.UserDefaultSettings.MCU == false)
					devicesToRemoveList.Add(device);
				else if (device.DeviceType == DeviceTypesEnum.MCU_B2B && _eolSettings.UserDefaultSettings.MCU_B2B == false)
					devicesToRemoveList.Add(device);
				else if (device.DeviceType == DeviceTypesEnum.ZimmerPowerMeter && _eolSettings.UserDefaultSettings.ZimmerPowerMeter == false)
					devicesToRemoveList.Add(device);
				else if (device.DeviceType == DeviceTypesEnum.NI_6002 && _eolSettings.UserDefaultSettings.NI_6002 == false)
					devicesToRemoveList.Add(device);
				else if (device.DeviceType == DeviceTypesEnum.NI_6002_2 && _eolSettings.UserDefaultSettings.NI_6002_2 == false)
					devicesToRemoveList.Add(device);
				else if (device.DeviceType == DeviceTypesEnum.Printer_TSC && _eolSettings.UserDefaultSettings.Printer_TSC == false)
					devicesToRemoveList.Add(device);
				else if (device.DeviceType == DeviceTypesEnum.NumatoGPIO && _eolSettings.UserDefaultSettings.NumatoGPIO == false)
					devicesToRemoveList.Add(device);
			}

			foreach(DeviceData device in devicesToRemoveList)
				devicesList.Remove(device);
		}

		private void SettingsVM_SettingsWindowClosedEvent()
		{
			UpdateSetup();
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

			_setupSelectionVM.ButtonsVisibility = Visibility.Collapsed;

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
