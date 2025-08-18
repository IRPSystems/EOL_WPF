
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceHandler.Models;
using EOL.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;
using Services.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Virinco.WATS.Interface;
using Process = Virinco.WATS.Interface.Models.Process; 
namespace EOL.ViewModels
{
	public class SettingsAdminViewModel : ObservableObject , INotifyPropertyChanged
	{


		public EOLSettings EolSettings { get; set; }

        public string SoftwarePath => EolSettings?.UserDefaultSettings?.DefaultMainSeqConfigFile ?? string.Empty;

        public string SoftwareFilename =>
            string.IsNullOrWhiteSpace(SoftwarePath) ? string.Empty : Path.GetFileName(SoftwarePath);

        public string SoftwareVersion => GetFileVersion(); // or GetFileVersion()

        public ObservableCollection<Process> TestOperations { get; } = new();
        private Process _selectedTestOperation;
        public Process SelectedTestOperation
        {
            get => _selectedTestOperation;
            set
            {
                if(_selectedTestOperation!=value)
                {
                    _selectedTestOperation = value;
                    OnPropertyChanged();
                }
            }
        }
        public SettingsAdminViewModel(
			EOLSettings eolSettings)
		{
			EolSettings = eolSettings;
			BrowseMCUParametersJsonPathCommand = new RelayCommand(BrowseMCUParametersJsonPath);
            SelectedTestOperation = new Process();
            _ = LoadProcessesOnceAsync(eolSettings.UserDefaultSettings.WatsTestCode ?? string.Empty);




        }

        // Async method
        public async Task LoadProcessesOnceAsync(string code)
        {
            try
            {
                // 1) fetch off the UI thread
                var list = await Task.Run(() => TDM.GetProcesses());

                // 2) update the bound collection on the UI thread
                Application.Current.Dispatcher.Invoke(() =>
                {
                    TestOperations.Clear();
                    foreach (var p in list)
                    {
                        if (p != null) TestOperations.Add(p);
                    }
                });

                if (!(code.IsNullOrEmpty()))
                    SelectedTestOperation = TestOperations.FirstOrDefault(p => p?.Code.ToString() == code);
            }
            catch (Exception ex)
            {
                LoggerService.Error(this, ex.Message);
            }
        }

        private void BrowseMCUParametersJsonPath()
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.InitialDirectory = Path.GetDirectoryName(
				EolSettings.DeviceSetupUserData.MCUJsonPath);
			bool? result = openFileDialog.ShowDialog();
			if (result != true)
				return;

			EolSettings.DeviceSetupUserData.MCUJsonPath = openFileDialog.FileName;
			LoadDevicesContainer?.Invoke();
		}

        public string GetFileVersion()
        {
            return Assembly.GetEntryAssembly()?
                .GetCustomAttribute<AssemblyFileVersionAttribute>()?
                .Version ?? "Unknown";
        }

        public void OnMainScriptChanged()
        {
            OnPropertyChanged(nameof(SoftwarePath));
            OnPropertyChanged(nameof(SoftwareFilename));
        }

        public RelayCommand BrowseMCUParametersJsonPathCommand { get; private set; }

		public event Action LoadDevicesContainer;
	}
}
