
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using DeviceHandler.Models;
using DeviceHandler.Models.DeviceFullDataModels;
using EOL.Models;
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
using System.Reflection.Metadata;
using System.Windows;
using static System.Net.Mime.MediaTypeNames;

namespace EOL.ViewModels
{
	public class RunViewModel:ObservableObject
	{
		#region Properties

		public enum RunStateEnum { None, Running, Ended, Aborted, Failed }

		public int RunPercentage { get; set; }

		public RunStateEnum RunState { get; set; }

		public ObservableCollection<string> TerminalTextsList { get; set; }

		public bool IsRunButtonEnabled { get; set; }

		public TimeSpan RunTime { get; set; }

		public RunScriptService RunScript { get; set; }

		public ScriptDiagramViewModel ScriptDiagram { get; set; }

		public bool IsAdminMode { get; set; }

		public Visibility ContinueVisibility { get; set; }


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

		private OpenProjectForRunService _openProject;
		private StopScriptStepService _stopScriptStep;
		private ObservableCollection<GeneratedProjectData> _generatedProjectsList;
		private GeneratedScriptData _stoppedScript; // TODO: initiate
		private ObservableCollection<DeviceParameterData> _logParametersList;
		private string MainScriptReportSubFolder = "\\Main Script Reports";
        private string MonitorLogSubFolder = "\\Monitor Logs";

        private ScriptStepSetParameter _stepSetParameter;

		#endregion Fields

		#region Constructor

		public RunViewModel(
			DevicesContainer devicesContainer,
			RunData runData,
			UserDefaultSettings userDefaultSettings,
			SettingsViewModel settingsViewModel)
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

				RunPercentage = 0;
				TerminalTextsList = new ObservableCollection<string>();
				RunState = RunStateEnum.None;

				DeviceFullData mcuDeviceFullData = _devicesContainer.TypeToDevicesFullData[Entities.Enums.DeviceTypesEnum.MCU];
				_stepSetParameter = new ScriptStepSetParameter()
				{
					Parameter = new MCU_ParamData()
					{
						Name = "Pass/Fail indication",
						Cmd = "eolpassflag"
					},

					Communicator = mcuDeviceFullData.DeviceCommunicator,
				};

				_stopScriptStep = new StopScriptStepService();
				RunScript = new RunScriptService(_logParametersList, _devicesContainer, _stopScriptStep, null);
				RunScript.ScriptStartedEvent += RunScript_ScriptStartedEvent;
				RunScript.CurrentStepChangedEvent += RunScript_CurrentStepChangedEvent;
				//RunScript.AbortScriptPath = @"C:\Users\smadar\Documents\Scripts\Tests\Empty Script.scr";


				ScriptDiagram = new ScriptDiagramViewModel();

				_openProject = new OpenProjectForRunService();
				_runProjectsList = new RunProjectsListService(null, RunScript, _devicesContainer);
				_runProjectsList.RunEndedEvent += _runProjectsList_ScriptEndedEvent;
				_generatedProjectsList = null;

				_generatedProjectsList = new ObservableCollection<GeneratedProjectData>();

				_flashingHandler = new FlashingHandler(devicesContainer);


                RegisterEvents();

				_settingsViewModel.LoadUserConfigToSettingsView();
			}
			catch (Exception ex)
			{
				LoggerService.Error(this, "C'tor failed", ex);
			}
		}

		#endregion Constructor

		#region Methods

		#region settingsViewModel events

		private void RegisterEvents()
		{
			_settingsViewModel.MainScriptEventChanged += LoadMainScriptFromPath;
            _settingsViewModel.SubScriptEventChanged += LoadSubScriptFromPath;
            _settingsViewModel.AbortScriptEventChanged += LoadAbortScriptFromPath;
            _settingsViewModel.MonitorScriptEventChanged += LoadMonitorFromPath;
        }

		

		private void LoadMainScriptFromPath()
        {
			if (String.IsNullOrEmpty(_userDefaultSettings.DefaultMainSeqConfigFile))
			{
				return;
            }
            LoadProject(_userDefaultSettings.DefaultMainSeqConfigFile);
        }

        private void LoadSubScriptFromPath()
        {
            if (String.IsNullOrEmpty(_userDefaultSettings.DefaultSubScriptFile))
            {
                return;
            }
            LoadProject(_userDefaultSettings.DefaultSubScriptFile);
        }

        private void LoadMonitorFromPath()
		{
            if (String.IsNullOrEmpty(_userDefaultSettings.DefaultMonitorLogScript))
            {
                return;
            }
            string jsonString = System.IO.File.ReadAllText(_userDefaultSettings.DefaultMonitorLogScript);

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            settings.TypeNameHandling = TypeNameHandling.All;
            _logParametersList = JsonConvert.DeserializeObject(jsonString, settings) as ObservableCollection<DeviceParameterData>;
			RunScript.ParamRecording.RecordDirectory = _userDefaultSettings.ReportsSavingPath + MonitorLogSubFolder;
        }

        private void LoadAbortScriptFromPath()
        {
            if (String.IsNullOrEmpty(_userDefaultSettings.DefaultAbortScriptFile))
            {
                return;
            }
            RunScript.AbortScriptPath = _userDefaultSettings.DefaultAbortScriptFile;
            _stoppedScript = _openProject.GetSingleScript(_userDefaultSettings.DefaultAbortScriptFile, _devicesContainer, null);
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

			_totalNumOfSteps = 0;
			foreach (GeneratedProjectData project in _generatedProjectsList)
			{
				foreach (GeneratedScriptData scriptData in project.TestsList)
				{
					SetDataToScriptTool(scriptData.ScriptItemsList);
				}
			}

			ContinueVisibility = Visibility.Collapsed;

			RunScript.SelectMotor.SelectedController = new ControllerSettingsData();
			RunScript.SelectMotor.SelectedMotor = new MotorSettingsData();

			_runData.StartTime = DateTime.Now;
			IsRunButtonEnabled = false;
			RunState = RunStateEnum.Running;

			_timerDuration.Start();
			_startTime = DateTime.Now;

			_runProjectsList.StartAll(
				_generatedProjectsList,
				_userDefaultSettings.isRecordMonitor,
				_stoppedScript, _logParametersList);

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
			if (!CreateDirectories())
			{
                return false;
            }
            return true;
        }

        private bool CreateDirectories()
        {
            if(!Directory.Exists(_userDefaultSettings.ReportsSavingPath))
			{
				MessageBox.Show("Report path doesnt exist:" + _userDefaultSettings.ReportsSavingPath + "\r\n" + "Please choose a valid path");
				return false;
			}
			try
			{
				if (!Directory.Exists(_userDefaultSettings.ReportsSavingPath + MainScriptReportSubFolder))
				{
					Directory.CreateDirectory(_userDefaultSettings.ReportsSavingPath + MainScriptReportSubFolder);
				}
                if (!Directory.Exists(_userDefaultSettings.ReportsSavingPath + MonitorLogSubFolder))
                {
                    Directory.CreateDirectory(_userDefaultSettings.ReportsSavingPath + MonitorLogSubFolder);
                }
				return true;
            }
			catch (Exception ex)
			{
				MessageBox.Show("Reports folder creation exception:\r\n " + ex.Message);
                return false;
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

        private void LoadProject(string scriptPath)
		{
			GeneratedProjectData project = _openProject.Open(
				scriptPath,
				_devicesContainer,
                _flashingHandler,
				_stopScriptStep);
			if (project == null ||
				project.TestsList == null || project.TestsList.Count == 0)
			{
				LoggerService.Error(this, "Failed to open the script", "Error");
				return;
			}

			_generatedProjectsList.Add(project);
		}

		private void SetDataToScriptTool(
			ObservableCollection<IScriptItem> scriptItemsList)
		{
			foreach(IScriptItem scriptItem in scriptItemsList)
			{
                if (scriptItem is ISubScript subScript)
                {
					SetDataToScriptTool(subScript.Script.ScriptItemsList);
					continue;
				}

				_totalNumOfSteps++;

				if (scriptItem is ScriptStepEOLSendSN sn)
				{
					sn.SerialNumber = _runData.SerialNumber;
					//sn.UserSN = _runData. // TODO?
				}
				else if (scriptItem is ScriptStepEOLFlash flash)
				{
					SetFlashData(flash);
				}
			}
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
			_timerDuration.Stop();
			_runData.EndTime = DateTime.Now;


			IsRunButtonEnabled = true;

			if (stopeMode == ScriptStopModeEnum.Aborted)
				RunState = RunStateEnum.Aborted;
			else
				RunState = RunStateEnum.Ended;


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
				_stepSetParameter.Value = 0;
			}
			else
				_stepSetParameter.Value = 1;

			_stepSetParameter.Execute();
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

		#endregion Methods

		#region Commands

		public RelayCommand RunCommand { get; private set; }
		public RelayCommand AbortCommand { get; private set; }
		public RelayCommand ContinueCommand { get; private set; }

		#endregion Commands
	}
}
