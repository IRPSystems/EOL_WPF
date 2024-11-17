
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CsvHelper;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using DeviceHandler.Models;
using DeviceHandler.Models.DeviceFullDataModels;
using Entities.Enums;
using EOL.Models;
using EOL.Models.Config;
using EOL.Services;
using EOL.Views;
using FlashingToolLib.FlashingTools;
using Microsoft.VisualBasic.ApplicationServices;
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

		public Visibility ContinueVisibility { get; set; }

		public string ErrorMessage { get; set; }

		#endregion Properties

		#region Fields


		private DevicesContainer _devicesContainer;

		private int _totalNumOfSteps;
		private int _stepsCounter;

		private RunData _runData;

		private System.Timers.Timer _timerDuration;
		private DateTime _startTime;

		private UserDefaultSettings _userDefaultSettings;

		private FlashingHandler _flashingHandler;

		private SettingsViewModel _settingsViewModel;

        private RunProjectsListService _runProjectsList;

		//Create also a backup list of list TestResults
		private RunResult _singleTestResult;

		private CSVWriter _csvWritter;

		private PDF_Creator _pdfCreator;

		private PrintFileHandler _printFileParser;

		private GeneratedScriptData _SafetyScript;

		private OpenProjectForRunService _openProject;
		private StopScriptStepService _stopScriptStep;
		private ObservableCollection<GeneratedProjectData> _generatedProjectsList;
		private GeneratedScriptData _stoppedScript; // TODO: initiate
        private GeneratedScriptData _abortScript;
        private ObservableCollection<DeviceParameterData> _logParametersList;
		private string MainScriptReportSubFolder = "Main Script Reports";
        private string MonitorLogSubFolder = "Monitor Logs";

        private ScriptStepSetParameter _stepSetParameter;

		private AdminView _adminView;
		private AdminViewModel _adminVM;

		private GeneratedProjectData _projectMain;
		private GeneratedProjectData _projectSub;

		#endregion Fields

		#region Constructor

		public RunViewModel(
			DevicesContainer devicesContainer,
			RunData runData,
			UserDefaultSettings userDefaultSettings,
			SettingsViewModel settingsViewModel,
			LogLineListService logLineList)
		{
            _settingsViewModel = settingsViewModel;
            _devicesContainer = devicesContainer;
			_runData = runData;
            _userDefaultSettings = userDefaultSettings;

            try
            {
				//PackageConfig
				PackageJsonFileGenerator.GenerateJsonFile();
                _timerDuration = new System.Timers.Timer(300);
				_timerDuration.Elapsed += _timerDuration_Elapsed;

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
				//RunScript.AbortScriptPath = @"C:\Users\smadar\Documents\Scripts\Tests\Empty Script.scr";


				ScriptDiagram = new ScriptDiagramViewModel();

				_openProject = new OpenProjectForRunService();
				_runProjectsList = new RunProjectsListService(RunScript, _devicesContainer);
				_runProjectsList.RunEndedEvent += _runProjectsList_ScriptEndedEvent;
				_runProjectsList.ErrorMessageEvent += RunProjectsList_ErrorMessageEvent;
				
				_generatedProjectsList = new ObservableCollection<GeneratedProjectData>();

				_adminVM = new AdminViewModel(
					ScriptDiagram,
					RunScript.MainScriptLogger,
					_generatedProjectsList);

				_flashingHandler = new FlashingHandler(devicesContainer);

                _singleTestResult = new RunResult();

				_csvWritter = new CSVWriter();

				_pdfCreator = new PDF_Creator();

                _printFileParser = new PrintFileHandler();

                RegisterEvents();

				_settingsViewModel.LoadUserConfigToSettingsView();

				LoadPrintFile();

				
			}
			catch (Exception ex)
			{
				LoggerService.Error(this, "C'tor failed", ex);
                MessageBox.Show(ex.ToString());
            }
		}

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

		#endregion Constructor

		#region Methods

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
			RunScript.ParamRecording.RecordDirectory = _userDefaultSettings.ReportsSavingPath + MonitorLogSubFolder;
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

        #endregion settingsViewModel events

        #region Running script events

        private void RunScript_ScriptStartedEvent()
		{
			ScriptDiagram.DrawScript(RunScript.CurrentScript.CurrentScript);
		}

		private void _runProjectsList_ScriptEndedEvent(
			ScriptStopModeEnum stopeMode,
			GeneratedScriptData endedScript)
		{
			if (stopeMode == ScriptStopModeEnum.Ended)
				RunPercentage = 100;

			Stop(stopeMode);

			PostRunActions();

			if(stopeMode == ScriptStopModeEnum.Aborted)
				RunState = RunStateEnum.Aborted;
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
			_stepsCounter++;
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

  //      private void LoadProject(string scriptPath, out GeneratedProjectData project)
		//{

		//	GeneratedProjectData tempProject = _openProject.Open(
		//		scriptPath,
		//		_devicesContainer,
  //              _flashingHandler,
		//		_stopScriptStep);
		//	if (project == null ||
		//		project.TestsList == null || project.TestsList.Count == 0)
		//	{
		//		LoggerService.Error(this, "Failed to open the script", "Error");
		//		return;
		//	}

		//	_generatedProjectsList.Add(project);
		//}

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
					flash.UdsSequence = _userDefaultSettings.FirstFileUdsSequence;
					flash.RXId = _userDefaultSettings.FirstFileUdsRx.ToString("X");
					flash.TXId = _userDefaultSettings.FirstFileUdsTx.ToString("X");
				}
			}
			else if (flash.NumOfFlashFile == 1)
			{
				flash.FilePath = _userDefaultSettings.SecondFlashFilePath;
				if (flash.IsEolSource)
				{
					flash.UdsSequence = _userDefaultSettings.SecondFileUdsSequence;
					flash.RXId = _userDefaultSettings.SecondFileUdsRx.ToString("X");
					flash.TXId = _userDefaultSettings.SecondFileUdsTx.ToString("X");
				}
			}
		}

		#endregion Load project



		private void Abort()
		{
			RunScript.AbortScript("User Abort");
		}

		private void Stop(ScriptStopModeEnum stopeMode)
		{
			try
			{
				_timerDuration.Stop();
				_runData.EndTime = DateTime.Now;


				IsRunButtonEnabled = true;

			if (stopeMode == ScriptStopModeEnum.Aborted)
				RunState = RunStateEnum.Aborted;
			else
				RunState = RunStateEnum.Passed;

				List<EOLStepSummeryData> eolStepSummerysList = new List<EOLStepSummeryData>();
				foreach (GeneratedProjectData project in _generatedProjectsList)
				{
					foreach (GeneratedScriptData script in project.TestsList)
					{
						GetScriptEOLStepSummerys(
							script,
							eolStepSummerysList);
					}
				}

				int passed;
				int failed;
				GetPassFailed(
					eolStepSummerysList,
					out passed,
					out failed);

				_runData.NumberOfTested = passed + failed;
				_runData.NumberOfFailed = failed;
				_runData.NumberOfPassed = passed;

				if (failed > 0)
				{
					RunState = RunStateEnum.Failed;
					_singleTestResult.TestStatus = "Failed";
					_stepSetParameter.Value = 0;
				}
				else
				{
					_singleTestResult.TestStatus = "Passed";
					_stepSetParameter.Value = 1;
				}

				_stepSetParameter.Execute();

				_singleTestResult.SerialNumber = _runData.SerialNumber;
				_singleTestResult.PartNumber = _runData.PartNumber;
				_singleTestResult.OperatorName = _runData.OperatorName;
				_singleTestResult.Steps = eolStepSummerysList;
				_singleTestResult.StartTimeStamp = _runData.StartTime.ToString("dd-MMM-yyyy hh:mm:ss.fff");
				_singleTestResult.EndTimeStamp = _runData.EndTime.ToString("dd-MMM-yyyy hh:mm:ss.fff");

				_csvWritter.WriteTestResult(_singleTestResult);

				_pdfCreator.CreatePdf(_generatedProjectsList, _singleTestResult, _userDefaultSettings);
			}
			catch (Exception ex)
			{
				LoggerService.Error(this, "Faild to handle stop tasks", ex);
			}
		}

		private void _timerDuration_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			_runData.Duration = DateTime.Now - _startTime;
		}

		private void Continue()
		{
			RunScript.User_Next();
		}


		#region Handle EOL Step Summery

		private void GetScriptEOLStepSummerys(
			IScript script,
			List<EOLStepSummeryData> eolStepSummerysList)
		{
			foreach(IScriptItem item in script.ScriptItemsList)
			{
				if (item is ScriptStepBase stepBase)
					eolStepSummerysList.AddRange(stepBase.EOLStepSummerysList);

				if(item is ISubScript subScript)
				{
					GetScriptEOLStepSummerys(
						subScript.Script,
						eolStepSummerysList);
				}
			}
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

        private void GetPassFailed(
			List<EOLStepSummeryData> eolStepSummerysList,
			out int passed,
			out int failed)
		{
			passed = 0;
			failed = 0;
			foreach (EOLStepSummeryData item in eolStepSummerysList)
			{
				if (item.IsPass)
					passed++;
				else
					failed++;
			}
		}

		#endregion Handle EOL Step Summery

		private void RunProjectsList_ErrorMessageEvent(string errorMessage)
		{
			ErrorMessage = errorMessage;
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

		#endregion Methods

		#region Commands

		public RelayCommand RunCommand { get; private set; }
		public RelayCommand AbortCommand { get; private set; }
		public RelayCommand ContinueCommand { get; private set; }
		public RelayCommand ShowAdminCommand { get; private set; }

		#endregion Commands
	}
}
