
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceHandler.Models;
using EOL.Models;
using EOL_Tester.Classes;
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

		#endregion Properties

		#region Fields

		private GeneratedScriptData _currentScript;

		private DevicesContainer _devicesContainer;

		private int _totalNumOfSteps;
		private int _stepsCounter;

		private RunData _runData;

		private System.Timers.Timer _timerDuration;
		private DateTime _startTime;

		private UserDefaultSettings _userDefaultSettings;

		#endregion Fields

		#region Constructor

		public RunViewModel(
			string scriptPath,
			DevicesContainer devicesContainer,
			RunData runData,
			UserDefaultSettings userDefaultSettings)
		{
			_devicesContainer = devicesContainer;
			_runData = runData;
			_userDefaultSettings = userDefaultSettings;

			_timerDuration = new System.Timers.Timer(300);
			_timerDuration.Elapsed += _timerDuration_Elapsed;

			IsRunButtonEnabled = true;

			RunCommand = new RelayCommand(Run);
			AbortCommand = new RelayCommand(Abort);

			RunPercentage = 0;
			TerminalTextsList = new ObservableCollection<string>();
			RunState = RunStateEnum.None;

			StopScriptStepService stopScriptStep = new StopScriptStepService();
			RunScript = new RunScriptService(null, _devicesContainer, stopScriptStep, null);
			RunScript.ScriptEndedEvent += _runScriptService_ScriptEndedEvent;
			RunScript.ScriptStartedEvent += _runScriptService_ScriptStartedEvent;
			RunScript.CurrentStepChangedEvent += RunScript_CurrentStepChangedEvent;


			ScriptDiagram = new ScriptDiagramViewModel();

			OpenProjectForRunService openProject = new OpenProjectForRunService();
			GeneratedProjectData project = openProject.Open(
				scriptPath,
				devicesContainer,
				null,
				stopScriptStep);
			if (project == null ||
				project.TestsList == null || project.TestsList.Count == 0)
			{
				LoggerService.Error(this, "Failed to open the script", "Error");
				return;
			}

			_currentScript = project.TestsList[0];
		}

		

		#endregion Constructor

		#region Methods

		private void _runScriptService_ScriptStartedEvent()
		{
			if(_currentScript == null)
				return;

			_timerDuration.Start();
			_startTime = DateTime.Now;

			ScriptDiagram.DrawScript(_currentScript);
		}

		private void _runScriptService_ScriptEndedEvent(ScriptStopModeEnum stopeMode)
		{
			if (stopeMode == ScriptStopModeEnum.Ended)
				RunPercentage = 100;

			Stop(stopeMode);
		}

		private void RunScript_CurrentStepChangedEvent(ScriptStepBase obj)
		{
			RunPercentage = (int)(((double)_stepsCounter / (double)_currentScript.ScriptItemsList.Count) * 100);
			_stepsCounter++;
		}

		private void Run()
		{
			_runData.StartTime = DateTime.Now;
			IsRunButtonEnabled = false;
			RunState = RunStateEnum.Running;

			SetSNToScriptTool(_currentScript.ScriptItemsList);
			SetFlashFileToScriptTool(_currentScript.ScriptItemsList);
			RunScript.Run(null, _currentScript, null, false);

			RunPercentage = 0;
			_totalNumOfSteps = _currentScript.ScriptItemsList.Count + 1;
			_stepsCounter = 1;
		}		

		private void SetSNToScriptTool(
			ObservableCollection<IScriptItem> scriptItemsList)
		{
			foreach(IScriptItem scriptItem in scriptItemsList)
			{
                if (scriptItem is ISubScript subScript)
                {
					SetSNToScriptTool(subScript.Script.ScriptItemsList);
					continue;
				}

				if(scriptItem is ScriptStepEOLSendSN sn)
				{
					sn.SerialNumber = _runData.SerialNumber;
					//sn.UserSN = _runData. // TODO?
				}
            }
		}

		private void SetFlashFileToScriptTool(
			ObservableCollection<IScriptItem> scriptItemsList)
		{
			foreach (IScriptItem scriptItem in scriptItemsList)
			{
				if (scriptItem is ISubScript subScript)
				{
					SetFlashFileToScriptTool(subScript.Script.ScriptItemsList);
					continue;
				}

				if (scriptItem is ScriptStepEOLFlash flash)
				{// TODO: first - second?
					flash.FilePath = _userDefaultSettings.FirstFlashFilePath;
				}
			}
		}

		private void Abort()
		{
			RunScript.AbortScript("User Abort");
		}

		private void Stop(ScriptStopModeEnum stopeMode)
		{			
			_timerDuration.Stop();
			_runData.EndTime = DateTime.Now;
			_runData.Duration = RunScript.RunTime.RunTime;

			CountRunSteps();

			IsRunButtonEnabled = true;

			if (stopeMode == ScriptStopModeEnum.Aborted)
				RunState = RunStateEnum.Aborted;
			else
				RunState = RunStateEnum.Ended;


			List<EOLStepSummeryData> eolStepSummerysList = new List<EOLStepSummeryData>();
			GetScriptEOLStepSummerys(
				_currentScript,
				eolStepSummerysList);

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
				RunState = RunStateEnum.Failed;
		}

		private void CountRunSteps()
		{
			_runData.NumberOfTested = _currentScript.TotalRunSteps;

			_runData.NumberOfPassed = 0;
			_runData.NumberOfFailed = 0;

			CountPassFaileRunSteps(_currentScript);
		}

		private void CountPassFaileRunSteps(GeneratedScriptData script)
		{
			if (script == null)
				return;

			_runData.NumberOfPassed += script.PassRunSteps;
			_runData.NumberOfFailed += script.FailRunSteps;

			foreach (IScriptItem item in script.ScriptItemsList)
			{
				if(item is ISubScript subScript)
				{
					CountPassFaileRunSteps(subScript.Script as GeneratedScriptData);
				}
			}
		}

		private void _timerDuration_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			RunScript.RunTime.RunTime = DateTime.Now - _startTime;
		}

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

		#endregion Methods

		#region Commands

		public RelayCommand RunCommand { get; private set; }
		public RelayCommand AbortCommand { get; private set; }

		#endregion Commands
	}
}
