
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DBCFileParser.Services;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using DeviceHandler.Models;
using DeviceHandler.Models.DeviceFullDataModels;
using Entities.Enums;
using EOL.Models;
using EOL.Models.Config;
using EOL.Services;
using EOL.Views;
using Newtonsoft.Json;
using ScriptHandler.Interfaces;
using ScriptHandler.Models;
using ScriptHandler.Models.ScriptSteps;
using ScriptHandler.Services;
using ScriptHandler.ViewModels;
using ScriptRunner.Enums;
using ScriptRunner.Services;
using Services.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using TestersDB_Lib.Models;
using WatsConstants;
using static FlashingToolLib.FlasherService;
using static System.Net.Mime.MediaTypeNames;

namespace EOL.ViewModels
{
	public class RunViewModel:ObservableObject 
    {
		#region Properties

		public enum RunStateEnum { None, Running, Passed, Aborted, Failed }

		public int RunPercentage { get; set; }

		public RunStateEnum RunState { get; set; }

		public ObservableCollection<string> TerminalTextsList { get; set; }

		public bool IsRunButtonEnabled { get; set; }

		public TimeSpan RunTime { get; set; }

		public RunScriptService RunScript { get; set; }

		public ScriptDiagramViewModel ScriptDiagram { get; set; }

		public bool IsAdminMode { get; set; }

        private bool _writetoWatsEnabled;
        public bool WritetoWatsEnabled
        {
            get => _writetoWatsEnabled;
			set 
			{
                SetProperty(ref _writetoWatsEnabled, value);
                UpdateWritingToWatsSetting();
            }

        }
        public Visibility ContinueVisibility { get; set; }

		public string ErrorMessage { get; set; }
        public string OperatorErrorMessage { get; set; }

        #endregion Properties

        #region Fields


        private DevicesContainer _devicesContainer;

		private int _totalNumOfSteps;
		private int _stepsCounter;
		private bool isTestTerminated = false;
		private RunData _runData;

		private System.Timers.Timer _timerDuration;
		private DateTime _startTime;

		private UserDefaultSettings _userDefaultSettings;

		private FlashingHandler _flashingHandler;

		private SettingsViewModel _settingsViewModel;

        private RunProjectsListService _runProjectsList;

		//Create also a backup list of list TestResults
		//private RunResult _singleTestResult;

		private CSVWriter _csvWritter;
		private RunResultToWatsConverter _runResultToWatsConverter;

        //private PDF_Creator _pdfCreator;

        private PrintFileHandler _printFileParser;

		private GeneratedScriptData _SafetyScript;

		private OpenProjectForRunService _openProject;
		private StopScriptStepService _stopScriptStep;
		private ObservableCollection<GeneratedProjectData> _generatedProjectsList;
		private GeneratedScriptData _stoppedScript; 
        private GeneratedScriptData _abortScript;
        private ObservableCollection<DeviceParameterData> _logParametersList;
		private string MainScriptReportSubFolder = "Main Script Reports";
        private string MonitorLogSubFolder = "Monitor Logs";

        private ScriptStepSetParameter _stepSetParameter;

		private CommSendResLogCsvWriter _commSendResLogCsvWriter;

		private byte[] WatsBLOB;

        private AdminView _adminView;
		private AdminViewModel _adminVM;

		private GeneratedProjectData _projectMain;
		private GeneratedProjectData _projectSub;

		private SettingsData _settingsData;

		private List<DeviceTypesEnum> _currentScriptDeviceList;

        private List<CommSendResLog> commSendResLogs = new();

        #endregion Fields

        #region Constructor

        public RunViewModel(
			DevicesContainer devicesContainer,
			RunData runData,
			UserDefaultSettings userDefaultSettings,
			SettingsViewModel settingsViewModel,
			LogLineListService logLineList,
			SettingsData settingsData)
		{
            _settingsViewModel = settingsViewModel;
            _devicesContainer = devicesContainer;
			_runData = runData;
            _userDefaultSettings = userDefaultSettings;
			_settingsData = settingsData;

			_runData.NumberOfTested = 0;
			_runData.NumberOfFailed = 0;
			_runData.NumberOfPassed = 0;

            try
            {
				//PackageConfig
				PackageJsonFileGenerator.GenerateJsonFile();
                _timerDuration = new System.Timers.Timer(300);
				_timerDuration.Elapsed += _timerDuration_Elapsed;

				_currentScriptDeviceList = new List<DeviceTypesEnum>();

				_commSendResLogCsvWriter = new();

                IsRunButtonEnabled = true;
                ContinueVisibility = Visibility.Collapsed;

				RunCommand = new RelayCommand(Run);
				AbortCommand = new RelayCommand(Abort);
				ContinueCommand = new RelayCommand(Continue);
				ShowAdminCommand = new RelayCommand(ShowAdmin);

				RunPercentage = 0;
				TerminalTextsList = new ObservableCollection<string>();
				RunState = RunStateEnum.None;

				if (_devicesContainer.TypeToDevicesFullData.ContainsKey(Entities.Enums.DeviceTypesEnum.MCU))
				{
					DeviceFullData mcuDeviceFullData = 
						_devicesContainer.TypeToDevicesFullData[Entities.Enums.DeviceTypesEnum.MCU];
					_stepSetParameter = new ScriptStepSetParameter()
					{
						Parameter = new MCU_ParamData()
						{
							Name = "Pass/Fail indication",
							Cmd = "eolpassflag"
						},

						Communicator = mcuDeviceFullData.DeviceCommunicator,
					};
				}

                _stopScriptStep = new StopScriptStepService();
				RunScript = new RunScriptService(_devicesContainer, _stopScriptStep, null, logLineList);
				RunScript.ScriptStartedEvent += RunScript_ScriptStartedEvent;
				RunScript.CurrentStepChangedEvent += RunScript_CurrentStepChangedEvent;
				RunScript.StepEndedEvent += RunScript_StepEndedEvent;
				//RunScript.AbortScriptPath = @"C:\Users\smadar\Documents\Scripts\Tests\Empty Script.scr";


				ScriptDiagram = new ScriptDiagramViewModel();

				_openProject = new OpenProjectForRunService();
				_runProjectsList = new RunProjectsListService(RunScript, _devicesContainer);
				_runProjectsList.RunEndedEvent += _runProjectsList_ScriptEndedEvent;
				_runProjectsList.ErrorMessageEvent += RunProjectsList_ErrorMessageEvent;
                _runProjectsList.OperatorErrorMessageEvent += RunProjectsList_OperatorErrorMessageEvent;


                _generatedProjectsList = new ObservableCollection<GeneratedProjectData>();

				_adminVM = new AdminViewModel(
					ScriptDiagram,
					RunScript.MainScriptLogger,
					_generatedProjectsList);

				_flashingHandler = new FlashingHandler(devicesContainer);

				//_singleTestResult = new RunResult();
                _runResultToWatsConverter = new RunResultToWatsConverter();

                _csvWritter = new CSVWriter();

				//_pdfCreator = new PDF_Creator();

                _printFileParser = new PrintFileHandler();

                RegisterEvents();

				_settingsViewModel.LoadUserConfigToSettingsView();
				WritetoWatsEnabled = _userDefaultSettings.isWritingtoWatsEnabled;

				LoadPrintFile();

				
			}
			catch (Exception ex)
			{
				LoggerService.Error(this, "C'tor failed", ex);
                MessageBox.Show(ex.ToString());
            }
		}

		

		#endregion Constructor

		#region Methods

		private void LoadPrintFile()
		{
			//If user selected printer device, try loading & parse .prn file
			foreach (var item in _devicesContainer.DevicesList)
			{
				if (item.DeviceType == DeviceTypesEnum.Printer_TSC)
				{
					string printParserError;
					if (!_printFileParser.OpenPrintFile(out printParserError))
					{
						MessageBox.Show("Unable to open print file due to: " + printParserError + "\r\n" +
										"If you wish to use the printer:\r\n" +
										"Restart the app with a valid print file");
						LoggerService.Error(this, "Error loading print file");
					}
					else
					{
						LoggerService.Inforamtion(this, "Loaded & parsed print file successfully");
						break;
					}
				}
			}
		}

		public void ChangeDarkLight(bool isLightTheme)
		{
			ScriptDiagram.ChangeBackground(
					System.Windows.Application.Current.MainWindow.FindResource(
						"MahApps.Brushes.Control.Background") as SolidColorBrush);
		}

		#region settingsViewModel events

		private void RegisterEvents()
		{
            _settingsViewModel.ReportsPathEventChanged += ReportsPathChangeEvent;
            _settingsViewModel.MainScriptEventChanged += LoadMainScriptFromPath;
            _settingsViewModel.SubScriptEventChanged += LoadSubScriptFromPath;
			_settingsViewModel.SafetyScriptEventChanged += LoadSafetyScriptFromPath;
            _settingsViewModel.AbortScriptEventChanged += LoadAbortScriptFromPath;
            _settingsViewModel.MonitorScriptEventChanged += LoadMonitorFromPath;
			_settingsViewModel.FirstFlashFileEventChanged += FirstFlashFileChangedEvent;
			_settingsViewModel.SecondFlashFileEventChanged += SecondFlashFileChangedEvent;
        }

        private void ReportsPathChangeEvent()
        {
            if (String.IsNullOrEmpty(_userDefaultSettings.DefaultMainSeqConfigFile))
            {
                return;
            }

			string mainScriptSubFolder =
					Path.Combine(_userDefaultSettings.ReportsSavingPath, MainScriptReportSubFolder);
			if (!Directory.Exists(mainScriptSubFolder))
			{
				Directory.CreateDirectory(mainScriptSubFolder);
			}

			string monitorSubFolder =
				Path.Combine(_userDefaultSettings.ReportsSavingPath, MonitorLogSubFolder);
			if (!Directory.Exists(monitorSubFolder))
			{
				Directory.CreateDirectory(monitorSubFolder);

			}


			string fileName =
				"Tester Report - PackageName - "
                + DateTime.Now.ToString("yyyy-MM-dd")
                + ".csv";

			_csvWritter._csvFilePath = Path.Combine(mainScriptSubFolder, fileName);

		}

        private void LoadMainScriptFromPath()
        {
			if (string.IsNullOrEmpty(_userDefaultSettings.DefaultMainSeqConfigFile))
				return;

			_generatedProjectsList.Remove(_projectMain);

			GeneratedProjectData project = _openProject.Open(
				_userDefaultSettings.DefaultMainSeqConfigFile,
				_devicesContainer,
				_flashingHandler,
				_stopScriptStep);
			if (project == null ||
				project.TestsList == null || project.TestsList.Count == 0)
			{
				LoggerService.Error(this, "Failed to open the Main script", "Error");
				return;
			}

			_projectMain = project;
			_generatedProjectsList.Add(project);
		}

        private void LoadSubScriptFromPath()
        {
            if (string.IsNullOrEmpty(_userDefaultSettings.DefaultSubScriptFile))
                return;

			_generatedProjectsList.Remove(_projectSub);

			GeneratedProjectData project = _openProject.Open(
				_userDefaultSettings.DefaultMainSeqConfigFile,
				_devicesContainer,
				_flashingHandler,
				_stopScriptStep);
			if (project == null ||
				project.TestsList == null || project.TestsList.Count == 0)
			{
				LoggerService.Error(this, "Failed to open the Sub script", "Error");
				return;
			}

			_projectSub = project;
			_generatedProjectsList.Add(project);
		}

        private void LoadMonitorFromPath()
		{
            if (string.IsNullOrEmpty(_userDefaultSettings.DefaultMonitorLogScript))
                return;
            
            string jsonString = System.IO.File.ReadAllText(_userDefaultSettings.DefaultMonitorLogScript);

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            settings.TypeNameHandling = TypeNameHandling.All;
            _logParametersList = JsonConvert.DeserializeObject(jsonString, settings) as ObservableCollection<DeviceParameterData>;

			if (string.IsNullOrEmpty(_userDefaultSettings.ReportsSavingPath) == false)
			{
				RunScript.ParamRecording.RecordDirectory =
					Path.Combine(_userDefaultSettings.ReportsSavingPath, MonitorLogSubFolder);
			}
        }

        private void LoadSafetyScriptFromPath()
        {
            if (String.IsNullOrEmpty(_userDefaultSettings.DefaultSafetyScriptFile))
            {
                return;
            }
            LoadSingleScript(out _SafetyScript, _userDefaultSettings.DefaultSafetyScriptFile);
        }

        private void LoadAbortScriptFromPath()
        {
            if (String.IsNullOrEmpty(_userDefaultSettings.DefaultAbortScriptFile))
            {
                return;
            }
			LoadSingleScript(out _abortScript, _userDefaultSettings.DefaultAbortScriptFile);
        }

		private void LoadSingleScript(out GeneratedScriptData script, string scripPath)
		{
            script = _openProject.GetSingleScript(scripPath, _devicesContainer, null);
			if(script == null)
			{
				MessageBox.Show("Failded to load single script");
			}
        }

        private void FirstFlashFileChangedEvent()
        {
			if (!String.IsNullOrEmpty(_userDefaultSettings.FirstFlashFilePath))
			{
                eFlashingTool flashingTool = new eFlashingTool();

                if (_flashingHandler.SelectFlashingTool(ref flashingTool , _userDefaultSettings.FirstFlashFilePath))
				{
					if(flashingTool == eFlashingTool.UDS)
					{
						
						_flashingHandler.LoadUdsXML(_userDefaultSettings.FirstFlashFileCustomerProp);
                    }
                }
            }
        }

        private void SecondFlashFileChangedEvent()
        {
            if (!String.IsNullOrEmpty(_userDefaultSettings.SecondFlashFilePath))
            {
                eFlashingTool flashingTool = new eFlashingTool();

                if (_flashingHandler.SelectFlashingTool(ref flashingTool, _userDefaultSettings.SecondFlashFilePath))
                {
                    if (flashingTool == eFlashingTool.UDS)
                    {

                        _flashingHandler.LoadUdsXML(_userDefaultSettings.SecondFlashFileCustomerProp);
                    }
                }
            }
        }

        #endregion settingsViewModel events

        #region Running script events

        private void RunScript_ScriptStartedEvent()
		{
			if (RunScript.CurrentScript.CurrentScript == _abortScript)
				return;

			ScriptDiagram.DrawScript(RunScript.CurrentScript.CurrentScript);
		}

		private void _runProjectsList_ScriptEndedEvent(
			ScriptStopModeEnum stopMode,
			GeneratedScriptData endedScript)
		{
			if (stopMode == ScriptStopModeEnum.Stopped)
				_stoppedScript = endedScript;

			if (stopMode == ScriptStopModeEnum.Ended)
				RunPercentage = 100;
            RunResult singleTestResult = new RunResult();

			Reports watsReports;

            Stop(stopMode, ref singleTestResult,out watsReports);

            var staThread = new Thread(() =>
            {
                HandleTestData(singleTestResult, watsReports);
            });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();

            PostRunActions();

			if(stopMode == ScriptStopModeEnum.Aborted)
				RunState = RunStateEnum.Aborted;
		}

        private void HandleTestData(RunResult singleTestResult, Reports watsReports)
        {
            // Instantiate and show the SavingDataWindow
            var savingDataWindow = new SavingDataWindow();
            savingDataWindow.Show();

            // Allow the UI to update.
            savingDataWindow.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));

            // Write CSV data.
            _csvWritter.WriteTestResult(singleTestResult, _generatedProjectsList.ToList());
            savingDataWindow.SetProgress(25);
            savingDataWindow.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));

            // Create PDF.
            PDF_Creator _pdfCreator = new PDF_Creator();
            _pdfCreator.CreatePdf(_generatedProjectsList, singleTestResult, _userDefaultSettings);
            savingDataWindow.SetProgress(50);
            savingDataWindow.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));

			if(RunState != RunStateEnum.Passed)
			{
                // Write log CSV.
                _commSendResLogCsvWriter.WriteLog(commSendResLogs);
                savingDataWindow.SetProgress(75);
                savingDataWindow.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
            }

            // Optionally write XML if enabled.
            if (WritetoWatsEnabled)
                _runResultToWatsConverter.SaveRunResultToXml(watsReports, _userDefaultSettings.ReportsSavingPath, _runData.SerialNumber);

            savingDataWindow.SetProgress(100);
            savingDataWindow.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));

            // Close the progress window.
            savingDataWindow.Close();
        }

        private void PostRunActions()
        {
			_runData.SerialNumber = string.Empty;
        }

        private void RunScript_CurrentStepChangedEvent(ScriptStepBase step)
		{
			if (step is ScriptStepNotification)
				ContinueVisibility = Visibility.Visible;
			else
				ContinueVisibility = Visibility.Collapsed;

			if(_runProjectsList.RunScript.CurrentScript.CurrentScript == 
				_runProjectsList.AbortScript)
			{
				return;
			}

			RunPercentage = (int)(((double)_stepsCounter / (double)_totalNumOfSteps) * 100);
			if (RunPercentage > 100)
				RunPercentage = 100;

			_stepsCounter++;
		}

		private void RunScript_StepEndedEvent(ScriptStepBase obj)
		{
            commSendResLogs.Add(obj.CommSendResLog);
        }

		#endregion Running script events

		private void Run()
		{
			if(!PreRunValidations())
			{
				return;
			}

			if(_generatedProjectsList == null || _generatedProjectsList.Count == 0)
			{
				LoggerService.Error(this, "There is not project defined for running", "Error");
				return;
			}
			ErrorMessage = null;
            commSendResLogs.Clear();

            _commSendResLogCsvWriter.CreatLog(_runData.SerialNumber);

            isTestTerminated = false;
            _runData.StartTime = new DateTime();
			_runData.Duration = TimeSpan.Zero;
			_runData.EndTime = new DateTime();

			_runData.NumberOfTested++;
			OperatorErrorMessage = "";
			_totalNumOfSteps = 0;
			string path = _userDefaultSettings.ReportsSavingPath;
			path = Path.Combine(_userDefaultSettings.ReportsSavingPath, "Monitor Logs");
			foreach (GeneratedProjectData project in _generatedProjectsList)
			{

				project.RecordingPath = path;

				foreach (GeneratedScriptData scriptData in project.TestsList)
				{
					_totalNumOfSteps += SetDataToScriptTool(scriptData.ScriptItemsList);
                    foreach (GeneratedScriptData script in project.TestsList)
                    {
                        ClearEOLStepSummerys(script);
                    }
                }
			}

			ContinueVisibility = Visibility.Collapsed;

			//RunScript.SelectMotor.SelectedController = new ControllerSettingsData();
			//RunScript.SelectMotor.SelectedMotor = new MotorSettingsData();

			_runData.StartTime = DateTime.Now;
			IsRunButtonEnabled = false;
			RunState = RunStateEnum.Running;

			_timerDuration.Start();
			_startTime = DateTime.Now;
			_runProjectsList.AbortScript = _abortScript;


            _runProjectsList.StartAll(
				_generatedProjectsList,
				_userDefaultSettings.isRecordMonitor,
				_stoppedScript, 
				_SafetyScript,
				_logParametersList);

			RunPercentage = 0;
			_stepsCounter = 1;
		}

        #region Pre Run Validations
        private bool PreRunValidations()
		{
#if !DEBUG
			if (!CheckDeviceConnectivity())
			{
                return false;
            }
#endif

            if (!ValidateRequiredOperatorInfo())
            {
                return false;
            }
            if (!ValidateRequiredFiles())
            {
                return false;
            }
			if (!Directories())
			{
                return false;
            }
            return true;
        }

        private bool CheckDeviceConnectivity()
        {
			//string errorMsg = "Please connect this devices:\r\n";
			//bool isMissingConnection = false;
   //         foreach(DeviceFullData device in _devicesContainer.DevicesFullDataList)
			//{
			//	if(device.CommState != DeviceHandler.Enums.CommunicationStateEnum.Connected)
			//	{
			//		errorMsg += device.Device.Name + "\r\n";
   //                 isMissingConnection = true;
   //             }
   //         }
			//if (isMissingConnection)
			//{
			//	MessageBox.Show(errorMsg);
			//	return false;
			//}
			return true;
        }

		

		private bool CheckLoadedScriptUsedDeviceConnectivity()
		{
            foreach (GeneratedProjectData project in _generatedProjectsList)
            {
                foreach (GeneratedScriptData scriptData in project.TestsList)
                {
                    UpdatedCurrentUsedDeviceList(scriptData.ScriptItemsList);
                    foreach (GeneratedScriptData script in project.TestsList)
                    {

                    }
                }
            }
            return true;
		}

		private void UpdatedCurrentUsedDeviceList(
            ObservableCollection<IScriptItem> scriptItemsList)
        {
            foreach (IScriptItem scriptItem in scriptItemsList)
            {
                if (scriptItem is ISubScript subScript)
                {
                    int repeats = (subScript as ScriptStepSubScript).Repeats;
                    int subScript_totalNumOfSteps =
                        SetDataToScriptTool(subScript.Script.ScriptItemsList);
                    continue;
                }

                if (scriptItem is ScriptStepEOLSendSN sn)
                {

                    sn.SerialNumber = _runData.SerialNumber;
                }
				if(scriptItem is ScriptStepCompare compare)
				{
					if(!_currentScriptDeviceList.Contains(compare.Parameter.Device.DeviceType))
					{
                        _currentScriptDeviceList.Add(compare.Parameter.Device.DeviceType);
                    }
                }
                if (scriptItem is ScriptStepEOLPrint print)
                {
                    print.SerialNumber = _runData.SerialNumber;
                    print.ParamData.DataContent = _printFileParser.BuildPrinterCmd(print);
                }
                else if (scriptItem is ScriptStepEOLFlash flash)
                {
                    SetFlashData(flash);
                }
            }
        }

        private bool Directories()
        {
   //         if(!Directory.Exists(_csvWritter._csvFilePath))
			//{
			//	MessageBox.Show("Report path doesnt exist:" + _csvWritter._csvFilePath + "\r\n" + "Please choose a valid path");
			//	return false;
			//}
			try
			{
				

				if(IsFileInUse(_csvWritter._csvFilePath))
				{
					return false;
				}
				return true;
            }
			catch (Exception ex)
			{
				MessageBox.Show("Reports folder creation exception:\r\n " + ex.Message);
                return false;
			}
        }

        private bool IsFileInUse(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return false;
                }
                using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    // The file is not locked (not opened in another process).
                    return false;
                }
            }
            catch (IOException e)
            {
                if (e.Message.Contains("is being used"))
                {
                    MessageBox.Show("Report file is open, please close it to proceed");
                }
                else
                {
                    MessageBox.Show(e.Message);
                }

                return true;
            }
        }

        private bool ValidateRequiredOperatorInfo()
        {
            string ErrorMsg = "Missing operation info:\r\n";
            bool isValid = true;

            //Validate Directories Exists
            if (String.IsNullOrEmpty(_runData.OperatorName))
            {
                ErrorMsg += "Operator Name\r\n";
                isValid = false;
            }
            if (String.IsNullOrEmpty(_runData.SerialNumber))
            {
                ErrorMsg += "SerialNumber\r\n";
                isValid = false;
            }
            if (String.IsNullOrEmpty(_runData.OperatorName))
            {
                ErrorMsg += "Part Number\r\n";
                isValid = false;
            }
            if (!isValid)
            {
                MessageBox.Show(ErrorMsg);
                return false;
            }
            return true;
        }

        private bool ValidateRequiredFiles()
        {
            string ErrorMsg = "Missing Files&Paths:\r\n";
            bool isValid = true;

            //Validate Directories Exists
            if (String.IsNullOrEmpty(_userDefaultSettings.ReportsSavingPath))
            {
                ErrorMsg += "Reports export path\r\n";
                isValid = false;
            }
            if (String.IsNullOrEmpty(_userDefaultSettings.DefaultMainSeqConfigFile))
            {
                ErrorMsg += "Main script file\r\n";
                isValid = false;
            }
            if (String.IsNullOrEmpty(_userDefaultSettings.DefaultAbortScriptFile))
            {
                ErrorMsg += "Abort script file\r\n";
                isValid = false;
            }
            if (!isValid)
            {
                MessageBox.Show(ErrorMsg);
                return false;
            }
            return true;
        }
#endregion

		#region Load project


		private int SetDataToScriptTool(
			ObservableCollection<IScriptItem> scriptItemsList)
		{
			int totalNumOfSteps = 0;
			foreach (IScriptItem scriptItem in scriptItemsList)
			{
                if (scriptItem is ISubScript subScript)
                {
					int repeats = (subScript as ScriptStepSubScript).Repeats;
					int subScript_totalNumOfSteps = 
						SetDataToScriptTool(subScript.Script.ScriptItemsList);
					totalNumOfSteps += subScript_totalNumOfSteps * repeats;
					totalNumOfSteps += repeats;
					continue;
				}

				totalNumOfSteps++;
				if (scriptItem is ScriptStepEOLSendSN sn)
				{
					
					sn.SerialNumber = _runData.SerialNumber;
				}
				if (scriptItem is ScriptStepEOLPrint print)
				{
					print.SerialNumber = _runData.SerialNumber;
					print.ParamData = new();
                    print.ParamData.DataContent = _printFileParser.BuildPrinterCmd(print);
				}
				else if (scriptItem is ScriptStepEOLFlash flash)
				{
					SetFlashData(flash);
				}
			}

			return totalNumOfSteps;
		}

        private void SetFlashData(ScriptStepEOLFlash flash)
		{
			if (flash == null)
				return;

			if (flash.NumOfFlashFile == 0)
			{
				flash.FilePath = _userDefaultSettings.FirstFlashFilePath;
				if (flash.IsEolSource)
				{
					flash.Customer = _userDefaultSettings.FirstFlashFileCustomerProp;
				}
			}
			else if (flash.NumOfFlashFile == 1)
			{
				flash.FilePath = _userDefaultSettings.SecondFlashFilePath;
				if (flash.IsEolSource)
				{
					flash.Customer = _userDefaultSettings.SecondFlashFileCustomerProp;
				}
			}
		}

		#endregion Load project



		private void Abort()
		{
            OperatorErrorMessage += "User Abort";
            _runProjectsList.IsAbortClicked = true;
            isTestTerminated = true;
            RunScript.AbortScript("User Abort");
        }

		private void Stop(ScriptStopModeEnum stopeMode, ref RunResult singleTestResult ,out Reports watsreports)
		{
			watsreports = new Reports();

            try
            {
                _timerDuration.Stop();
				_runData.EndTime = DateTime.Now;

				IsRunButtonEnabled = true;

				if (stopeMode == ScriptStopModeEnum.Aborted)
				{
					RunState = RunStateEnum.Aborted;
					singleTestResult.StopReason = "Aborted";
				}
				else
					RunState = RunStateEnum.Passed;

				if (ErrorMessage != null)
					RunState = RunStateEnum.Aborted;


				if (RunState == RunStateEnum.Passed)
					singleTestResult.StopReason = "PASSED";

                string text = _commSendResLogCsvWriter.csvLine.ToString();

                Encoding encoding = Encoding.UTF8;

                WatsBLOB = encoding.GetBytes(text);

                List<Step> watsSteps = new List<Step>();

                string watsErrorMessage = string.Empty;

                Step ProjectStep = new Step
				{
					Steps = new List<Step>(),
                    StepType = StepTypes.SequenceCall,
                    Group = "Main"
                };

                ScriptStepBase failedStep = null;
				bool iserror = false;
				List<EOLStepSummeryData> eolStepSummerysList = new List<EOLStepSummeryData>();
				foreach (GeneratedProjectData project in _generatedProjectsList)
				{
                    ProjectStep.Name = project.Name;
					ProjectStep.Sequencecall = new SequenceCall
                    {
                        Name = project.Name
                    }; ;

                    if (WatsBLOB != null)
                    {
                        string base64string = Convert.ToBase64String(WatsBLOB);
                        Step step = new Step()
                        {
                            Name = "WATS BLOB",
                            StepType = "Attachment",
                            Attachments = new List<Attachment>()
                        };
                        step.Attachments.Add(new Attachment() { Name = "Test", Base64Data = base64string, ContentType = "text/csv" });
                        ProjectStep.Steps.Add(step);
                    }

                    //bool to determine wheter to write the test into report
                    bool isWritingtoWatsReport = true;
					//wats error message
					string errorMessage; 

					foreach (GeneratedScriptData script in project.TestsList)
					{
						//building test object for wats
						Step teststep = new Step
						{
							Group = "Main",
							Name = script.Name,
							Status = ((script.IsPass == true && script.isExecuted) ? StatusCodes.Passed : (script.isExecuted == true) ? StatusCodes.Failed : StatusCodes.Skipped),
							Sequencecall = new SequenceCall
							{
								Name = script.Name
							},
							Steps = new List<Step>(),
							StepType = StepTypes.SequenceCall
						};
						//Cumulative execution time for tests				
						double totalExecutionTime;
                        if ((script?.IsPass == false) && !OperatorErrorMessage.Contains("Test"))
							OperatorErrorMessage += "\r\nTest: " + script.Name;

						failedStep = GetScriptEOLStepSummerys(
							script,
							script,
							eolStepSummerysList,
							out totalExecutionTime,
							out errorMessage,
							ref iserror,
							ref teststep
							);

						if (script.IsPass == false)
							watsErrorMessage = errorMessage;

						teststep.TotalTime = totalExecutionTime;

                        if (script.IsPass == false )
                            isWritingtoWatsReport = false;

                        if (isWritingtoWatsReport || (script.isExecuted && !isWritingtoWatsReport))
							ProjectStep.Steps.Add(teststep);


                    }
                }


                singleTestResult.FailedStep = failedStep;
				singleTestResult.AppVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
				singleTestResult.RackNumber = _settingsData.RackNumber;

				bool isPassed = true;

				foreach(GeneratedProjectData project in _generatedProjectsList)
				{
					foreach(GeneratedScriptData script in project.TestsList)
					{
						isPassed &= (script.IsPass == true);
					}
				}

				if (isPassed)
					_runData.NumberOfPassed++;
				else
					_runData.NumberOfFailed++;

				if (_devicesContainer.TypeToDevicesFullData.ContainsKey(DeviceTypesEnum.MCU))
				{
					if (isPassed == false)
					{
						RunState = RunStateEnum.Failed;
						singleTestResult.TestStatus = "Failed";
						_stepSetParameter.Value = 0;
					}
					else
					{
						singleTestResult.TestStatus = "Passed";
						_stepSetParameter.Value = 1;
					}

					_stepSetParameter.Execute();
				}



                singleTestResult.SerialNumber = _runData.SerialNumber;
                singleTestResult.PartNumber = _runData.PartNumber;
                singleTestResult.OperatorName = _runData.OperatorName;
                singleTestResult.Steps = eolStepSummerysList;
                singleTestResult.StartTimeStamp = _runData.StartTime.ToString("dd-MMM-yyyy hh:mm:ss.fff");
                singleTestResult.EndTimeStamp = _runData.EndTime.ToString("dd-MMM-yyyy hh:mm:ss.fff");
				singleTestResult.StartTimeStamp8601 = _runData.StartTime.ToString("O");
				singleTestResult.ExecutionTime = _runData.Duration.TotalSeconds;

                ProjectStep.Status = singleTestResult.TestStatus;
                watsSteps.Add(ProjectStep);

                watsreports = new Reports
                {
                    Report = new Report
                    {
                        Type = "UUT",
                        Start = singleTestResult.StartTimeStamp8601,
                        Result = (isTestTerminated || _SafetyScript.IsPass == false) ? StatusCodes.Terminated : (iserror == true) ? StatusCodes.Error: singleTestResult.TestStatus,
                        SerialNumber = singleTestResult.SerialNumber,
                        PartNumber = singleTestResult.PartNumber,
                        MachineName = singleTestResult.RackNumber, // Set as needed
                        MiscInfo = new List<MiscInfo>(),

                        UUT = new Models.UUT
                        {
                            UserLoginName = singleTestResult.OperatorName,
                            ExecutionTime = singleTestResult.ExecutionTime, // Set as needed
                            ErrorMessage = singleTestResult.FailedStep?.ErrorMessage, // Set as needed
                        },
                        Process = new Process
                        {
                            Code = "22",
                            Name = "Or - Testing"
                        },

                        Steps = watsSteps
                    }
                };

                MiscInfo miscInfo = new MiscInfo
                {
                    Typedef = string.Empty,
                    Description = "Error Message",
                    Text = watsErrorMessage
                };
                watsreports.Report.MiscInfo.Add(miscInfo);


			}
			catch (Exception ex)
			{
				LoggerService.Error(this, "Faild to handle stop tasks", ex);
			}
		}

		private void _timerDuration_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			TimeSpan ellapsedTime = DateTime.Now - _startTime;
			_runData.Duration = ellapsedTime;
			RunScript.RunTime.RunTime = ellapsedTime;
		}

		private void Continue()
		{
			RunScript.User_Next();
		}


		#region Handle EOL Step Summery


		private ScriptStepBase GetScriptEOLStepSummerys(
			IScript script,
			IScript test,
			List<EOLStepSummeryData> eolStepSummerysList,
            out double totalExecutionTime,
			out string watsErrorMessage,
			ref bool iserror,
            ref Step watsStep
			)
		{
            watsErrorMessage = string.Empty; 
            totalExecutionTime = 0;
            ScriptStepBase failedStep = null;
			foreach (IScriptItem item in script.ScriptItemsList)
			{
				if (item is ScriptStepBase stepBase)
				{
                    //_runResultToWatsConverter.StepList.Add(stepBase);

					eolStepSummerysList.AddRange(stepBase.EOLStepSummerysList);

					stepBase.SubScriptName = script.Name;
					stepBase.TestName = test.Name;

					if (stepBase.OperatorErrorDescription != null && stepBase.OperatorErrorDescription != string.Empty )
						OperatorErrorMessage += "\r\nStep: "+ stepBase.UserTitle + " - " + stepBase.OperatorErrorDescription;

					if (!(item is ISubScript))
					{
						if (stepBase.IsPass == false)
						{
							failedStep = stepBase;
						}
						//Step Step = new Step
						//{
						//    Group = "Main",
						//    Name = stepBase.UserTitle,
						//    Status = (stepBase.IsPass == true ? "Passed" : stepBase.IsPass == false ? "Failed" : "Skipped"),
						//    StepType = "Action"
						//};
                        Step Step = _runResultToWatsConverter.HandleStep(stepBase);
						if (Step != null && !string.IsNullOrEmpty(Step.StepErrorMessage) && stepBase.IsPass == false)
							watsErrorMessage = Step.StepErrorMessage;
						if (Step != null)
						{
							totalExecutionTime += Step.TotalTime;
							watsStep.Steps.Add(Step);
						}

                        if (stepBase.IsError == true)
                            iserror = true;

                    }

                }

				if (item is ISubScript subScript)
				{
					Step subsctiptstep = new Step();

                    if (subScript is ScriptStepBase subscript)
                    {
                        subsctiptstep = new Step
                        {
                            Group = "Main",
                            Name = subscript.UserTitle ?? subscript.Name,
                            Status = ((subscript.IsPass == true && subscript.IsExecuted == true) ? StatusCodes.Passed : (subscript.IsExecuted == true) ? StatusCodes.Failed : StatusCodes.Skipped),
                            Sequencecall = new SequenceCall
                            {
                                Name = subscript.UserTitle ?? subscript.Name
                            },
                            Steps = new List<Step>(),
                            StepType = StepTypes.SequenceCall
                        };
                    }

					double cumulatedExecutionTime;
                    //               if (item is ScriptStepBase subscriptstep)
                    //_runResultToWatsConverter.SubScriptList.Add(subscriptstep);

                    if ((subScript.Script?.IsPass == false) && !OperatorErrorMessage.Contains("Sub Script"))
						OperatorErrorMessage += "\r\nSub Script: " + subScript.Script.Name;
					
					ScriptStepBase failedStepTemp = GetScriptEOLStepSummerys(
						subScript.Script,
						test,
						eolStepSummerysList,
						out cumulatedExecutionTime,
						out watsErrorMessage,
						ref iserror,
                        ref subsctiptstep);

					subsctiptstep.TotalTime = cumulatedExecutionTime;

					totalExecutionTime += subsctiptstep.TotalTime;

                    watsStep.Steps.Add(subsctiptstep);

					if (failedStepTemp != null)
						failedStep = failedStepTemp;
				}
			}

			return failedStep;
		}


        private void ClearEOLStepSummerys(
			IScript script)
        {
            foreach (IScriptItem item in script.ScriptItemsList)
            {
				if (item is ScriptStepBase stepBase)
					stepBase.EOLStepSummerysList.Clear();

                if (item is ISubScript subScript)
                {
                    ClearEOLStepSummerys(
                        subScript.Script);
                }
            }
        }

		#endregion Handle EOL Step Summery

		private void RunProjectsList_ErrorMessageEvent(string errorMessage)
		{
			ErrorMessage = errorMessage;
		}

        private void RunProjectsList_OperatorErrorMessageEvent(string errorMessage)
        {
            OperatorErrorMessage = errorMessage;
        }

        public void ShowAdmin()
		{
			if (_adminView == null || _adminView.IsVisible == false)
			{
				_adminView = new AdminView() { DataContext = _adminVM };

				_adminView.Show();
			}

			_adminView.Topmost = true;
			System.Threading.Thread.Sleep(100);
			_adminView.Topmost = false;
			_adminView.Focus();
		}

		public void CloseAdmin()
		{
			if (_adminView == null || _adminView.IsVisible == false)
				return;

			_adminView.Close();
		}

        public void UpdateWritingToWatsSetting()
        {
            _userDefaultSettings.isWritingtoWatsEnabled = WritetoWatsEnabled;
            // Save the updated settings if necessary
            // _userConfigManager.SaveConfig(_userDefaultSettings);
        }

        #endregion Methods

        #region Commands

        public RelayCommand RunCommand { get; private set; }
		public RelayCommand AbortCommand { get; private set; }
		public RelayCommand ContinueCommand { get; private set; }
		public RelayCommand ShowAdminCommand { get; private set; }

		#endregion Commands
    }
}
