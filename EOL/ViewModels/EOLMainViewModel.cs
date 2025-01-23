
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
using System.Windows.Controls;
using System.Windows.Media;
using DeviceCommunicators.MCU;
using DeviceCommunicators.PowerSupplayEA;
using DeviceCommunicators.NI_6002;
using Syncfusion.DocIO.DLS;
using TestersDB_Lib;

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

		public DatabaseHandler _databaseHandlerObj { get; private set; }
		public List<ModeType> ModeTypeList { get; set; }

		public string SelectedMode { get; set; }

		#endregion Properties

		#region Fields

		private EOLSettings _eolSettings;

		//private UserDefaultSettings _eolSettings.UserDefaultSettings;

		private ReadDevicesFileService _readDevicesFile;

		private SetupSelectionViewModel _setupSelectionVM;

        private NI6002_Init _initNI;

        private UserConfigManager _userConfigManager;

		private RunData _runData;

		private CommunicationWindowView _communicationWindowView;
		private SettingsView _settingsView;

		private RichTextBox _richTextBox;

		private string AdminPassword = "2512";

		private LogLineListService _logLineList;

		private bool? _isConfigSelectedByUser;

		#endregion Fields

		#region Constructor

		public EOLMainViewModel()
		{
			_logLineList = new LogLineListService();

			_userConfigManager = new UserConfigManager();

			_eolSettings = EOLSettings.LoadEOLUserData("EOL");

			_runData = new RunData();

			_databaseHandlerObj = new DatabaseHandler();

			_isConfigSelectedByUser = _userConfigManager.ReadConfig(_eolSettings);

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
			OperatorVM.Run.CloseAdmin();

			if(_logLineList != null)
				_logLineList.Dispose();
		}

		#region Load

		private void Loaded()
		{
			try
			{

				_richTextBox = new RichTextBox();
				_richTextBox.Background = Brushes.Black;
				_richTextBox.TextChanged += _richTextBox_TextChanged;

				LoggerService.Init("EOL.log", Serilog.Events.LogEventLevel.Information, _richTextBox);
				LoggerService.Inforamtion(this, "-------------------------------------- EOL ---------------------");


				LoggerService.Inforamtion(this, "Starting Loaded of EOLMainViewModel");


                _initNI = new NI6002_Init(_logLineList);


                _readDevicesFile = new ReadDevicesFileService();
				_setupSelectionVM =
					new SetupSelectionViewModel(_eolSettings.DeviceSetupUserData, _readDevicesFile);
				InitSetupView();

				MergeATEParamsToMCU();



				if (_isConfigSelectedByUser == null)
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
				else if (_eolSettings.DeviceSetupUserData.SetupDevicesList == null ||
					_eolSettings.DeviceSetupUserData.SetupDevicesList.Count == 0)
				{
					_eolSettings.DeviceSetupUserData.SetupDevicesList = new ObservableCollection<DeviceTypesEnum>();
					_eolSettings.DeviceSetupUserData.SetupDevicesList.Add(DeviceTypesEnum.MCU);
					_eolSettings.DeviceSetupUserData.SetupDevicesList.Add(DeviceTypesEnum.MCU_2);
					_eolSettings.DeviceSetupUserData.SetupDevicesList.Add(DeviceTypesEnum.NI_6002);
					_eolSettings.DeviceSetupUserData.SetupDevicesList.Add(DeviceTypesEnum.NI_6002_2);
					_eolSettings.DeviceSetupUserData.SetupDevicesList.Add(DeviceTypesEnum.Printer_TSC);
					_eolSettings.DeviceSetupUserData.SetupDevicesList.Add(DeviceTypesEnum.NumatoGPIO);
					_eolSettings.DeviceSetupUserData.SetupDevicesList.Add(DeviceTypesEnum.PowerSupplyEA);

					foreach(DeviceData device in _setupSelectionVM.DevicesSourceList)
						_setupSelectionVM.DevicesList.Add(device);
					
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
					_eolSettings, 
					_userConfigManager,
					_setupSelectionVM,
					_databaseHandlerObj
					);
                SettingsVM.SettingsWindowClosedEvent += SettingsVM_SettingsWindowClosedEvent;
				SettingsVM.SettingsAdminVM.LoadDevicesContainer += SettingsAdminVM_LoadDevicesContainer;

				OperatorVM = new OperatorViewModel(
					DevicesContainter, 
					_eolSettings.ScriptUserData,
					_eolSettings.UserDefaultSettings, 
					SettingsVM, 
					_runData,
					_richTextBox,
					_logLineList,
                    _databaseHandlerObj);

				ChangeDarkLight();

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

		private void SettingsAdminVM_LoadDevicesContainer()
		{
			if (DevicesContainter.TypeToDevicesFullData.ContainsKey(DeviceTypesEnum.MCU) == false)
				return;

			ObservableCollection<DeviceData> devicesList = new ObservableCollection<DeviceData>();
			_readDevicesFile.ReadFromMCUJson(
				_eolSettings.DeviceSetupUserData.MCUJsonPath,
				devicesList,
				"MCU",
				DeviceTypesEnum.MCU);

			DeviceFullData deviceFullData = DevicesContainter.TypeToDevicesFullData[DeviceTypesEnum.MCU];
			deviceFullData.Device = devicesList[0];

			DeviceData mcuDevice = DevicesContainter.DevicesList.ToList().Find((d) => d.Name == "MCU");
			if (mcuDevice == null)
				return;

			int index = DevicesContainter.DevicesList.IndexOf(mcuDevice);
			if (index < 0)
				return;

			DevicesContainter.DevicesList[index] = devicesList[0];

		}

		private void MergeATEParamsToMCU()
		{
			// Read the ATE json
			ObservableCollection<DeviceData> deviceList_ATE = new ObservableCollection<DeviceData>();
			_readDevicesFile.ReadFromATEJson(@"Data\Device Communications\ATE.json", deviceList_ATE);
			if (deviceList_ATE == null || deviceList_ATE.Count == 0)
				return;

			MCU_DeviceData ateDevice = deviceList_ATE[0] as MCU_DeviceData;
			if (ateDevice == null)
				return;

			MCU_DeviceData mcuDevice =
				_setupSelectionVM.DevicesSourceList_Full.ToList().Find((d) => d.DeviceType == DeviceTypesEnum.MCU) as MCU_DeviceData;
			if (mcuDevice == null) 
				return;

			mcuDevice.MCU_GroupList.Add(ateDevice.MCU_GroupList[0]);
			
			foreach (var param in ateDevice.MCU_FullList)
			{
				param.DeviceType = mcuDevice.DeviceType;
				param.Device = mcuDevice;
				mcuDevice.MCU_FullList.Add(param);
			}
		}

		private void _richTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			_richTextBox.ScrollToEnd();
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
					device.DeviceType != DeviceTypesEnum.MCU_2 &&
					device.DeviceType != DeviceTypesEnum.ZimmerPowerMeter &&
					device.DeviceType != DeviceTypesEnum.NI_6002 &&
					device.DeviceType != DeviceTypesEnum.NI_6002_2 &&
					device.DeviceType != DeviceTypesEnum.Printer_TSC &&
					device.DeviceType != DeviceTypesEnum.NumatoGPIO &&
					device.DeviceType != DeviceTypesEnum.PowerSupplyEA)
				{
					devicesToRemoveList.Add(device);
					continue;
				}

				if (device.DeviceType == DeviceTypesEnum.MCU && _eolSettings.UserDefaultSettings.MCU == false)
					devicesToRemoveList.Add(device);
				if (device.DeviceType == DeviceTypesEnum.MCU_2 && _eolSettings.UserDefaultSettings.MCU_2 == false)
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
				else if (device.DeviceType == DeviceTypesEnum.PowerSupplyEA && _eolSettings.UserDefaultSettings.PowerSupplyEA == false)
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


            if (DevicesContainter.TypeToDevicesFullData.ContainsKey(DeviceTypesEnum.NI_6002) ||
                DevicesContainter.TypeToDevicesFullData.ContainsKey(DeviceTypesEnum.NI_6002_2))
            {
                _initNI.BindDevices();
            }


            foreach (DeviceData device in newDevices)
			{
				DeviceFullData deviceFullData = DeviceFullData.Factory(device);

				deviceFullData.Init("EOL", _logLineList);

                if (device.DeviceType == Entities.Enums.DeviceTypesEnum.NI_6002 &&
					string.IsNullOrEmpty(_initNI.NI_a) == false)
                {
                    (deviceFullData.ConnectionViewModel as NI6002ConncetViewModel).DeviceName = _initNI.NI_a;
                }
                else if (device.DeviceType == Entities.Enums.DeviceTypesEnum.NI_6002_2 &&
						 string.IsNullOrEmpty(_initNI.NI_b) == false)
                {
                    (deviceFullData.ConnectionViewModel as NI6002ConncetViewModel).DeviceName = _initNI.NI_b;
                }

                if (deviceFullData.Device.DeviceType == DeviceTypesEnum.PowerSupplyEA && deviceFullData.DeviceCommunicator is PowerSupplayEA_Communicator)
				{
					(deviceFullData.DeviceCommunicator as PowerSupplayEA_Communicator).SetIsUseRampForOnOff(false);
				}


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
			OperatorVM.ChangeDarkLight(_eolSettings.IsLightTheme);
		}

		private void InitCommunicationSettings()
		{
            if (DevicesContainter.TypeToDevicesFullData.ContainsKey(DeviceTypesEnum.NI_6002) ||
                DevicesContainter.TypeToDevicesFullData.ContainsKey(DeviceTypesEnum.NI_6002_2))
            {

                _initNI.BindDevices();


                foreach (DeviceFullData device in DevicesContainter.DevicesFullDataList)
                {
                    if (device.Device.DeviceType == DeviceTypesEnum.NI_6002 &&
                        string.IsNullOrEmpty(_initNI.NI_a) == false)
                    {
                        (device.ConnectionViewModel as NI6002ConncetViewModel).DeviceName = _initNI.NI_a;
                    }
                    else if (device.Device.DeviceType == DeviceTypesEnum.NI_6002_2 &&
                        string.IsNullOrEmpty(_initNI.NI_a) == false)
                    {
                        (device.ConnectionViewModel as NI6002ConncetViewModel).DeviceName = _initNI.NI_b;
                    }
                }
            }

            if (_communicationWindowView == null || _communicationWindowView.IsVisible == false)
			{
				_communicationWindowView = new CommunicationWindowView()
				{
					DataContext = CommunicationSettings,
					Owner = Application.Current.MainWindow
				};

				_communicationWindowView.Show();
			}

			_communicationWindowView.Topmost = true;
			System.Threading.Thread.Sleep(100);
			_communicationWindowView.Topmost = false;
			_communicationWindowView.Focus();
		}

		private void Settings()
		{
			if (_settingsView == null || _settingsView.IsVisible == false)
			{
				_settingsView = new SettingsView()
				{
					DataContext = SettingsVM,
					Owner = Application.Current.MainWindow
				};

				_setupSelectionVM.ButtonsVisibility = Visibility.Collapsed;

				_settingsView.Show();
			}

			_settingsView.Topmost = true;
			System.Threading.Thread.Sleep(100);
			_settingsView.Topmost = false;
			_settingsView.Focus();
		}

        private void ModesDropDownMenuItem(string mode)
        {
            switch (mode)
            {
                case "Admin":
                    var passwordWindow = new PasswordWindow();
                    if (passwordWindow.ShowDialog() == true)
                    {
                        // Check if the entered password is correct
                        if (passwordWindow.Password == AdminPassword) // Replace with your actual password
                        {
                            SelectedMode = "Admin";
                            OperatorVM.Run.IsAdminMode = true;
                            OperatorVM.Run.ShowAdmin();

							SettingsVM.IsAdminMode = true;
                        }
                        else
                        {
                            MessageBox.Show("Incorrect password", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }

                    break;
                case "Operator":
                    SelectedMode = "Operator";
                    OperatorVM.Run.IsAdminMode = false;
					SettingsVM.IsAdminMode = false;
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
