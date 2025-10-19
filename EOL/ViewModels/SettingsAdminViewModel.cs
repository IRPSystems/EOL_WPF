
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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Virinco.Newtonsoft.Json;
using Virinco.WATS.Interface;
using Virinco.WATS.Interface.MES.Software;
using Virinco.WATS.Service.MES.Contract;
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

        public string PendingPackageName { get; set; }            // set when a pending pkg is selected

        private string _approvedBy;
        public string ApprovedBy
        {
            get => _approvedBy;
            set
            {
                if (_approvedBy == value) return;
                _approvedBy = value;
                OnPropertyChanged(nameof(ApprovedBy));
                UpdateCanRelease();
            }
        }
        private bool _canReleasePendingPackage;
        public bool CanReleasePendingPackage
        {
            get => _canReleasePendingPackage;
            private set
            {
                if (_canReleasePendingPackage == value) return;
                _canReleasePendingPackage = value;
                OnPropertyChanged(nameof(CanReleasePendingPackage));
            }
        }
        public bool IsPackagePending { get; set; }

        public readonly TDM _tdm;
        private Package SelectedPackage { get; set; }

        private string? _machineLocation;
        public string MachineLocation => _machineLocation ??=
            (string.IsNullOrWhiteSpace(GetMachineLocation()) ? "Unknown" : GetMachineLocation());

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
			EOLSettings eolSettings,
            TDM tdm,
            Package selectedpackage)
		{
			EolSettings = eolSettings;
			BrowseMCUParametersJsonPathCommand = new RelayCommand(BrowseMCUParametersJsonPath);
            ReleasePackageCommand = new RelayCommand(ReleasePackage);
            SelectedTestOperation = new Process();
            _ = LoadProcessesOnceAsync(eolSettings.UserDefaultSettings.WatsTestCode ?? string.Empty);
            _tdm = tdm;

            if (selectedpackage!= null && selectedpackage.Status == (int)StatusEnum.Pending)
            {
                LoadPackageData(selectedpackage);
                IsPackagePending = true;
                SelectedPackage = selectedpackage;
            }
            else
            {
                IsPackagePending = false;
            }
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

        private string GetMachineLocation()
        {
            if (_tdm == null)
                return string.Empty;
            var machinelocation = _tdm?.Location ?? string.Empty;
            if (machinelocation == null)
                return string.Empty;
            return machinelocation ?? string.Empty;
        }

        private void LoadPackageData(Package package)
        {
            PendingPackageName = package?.Name ?? string.Empty;
        }

        private async void ReleasePackage()
        {
            if (SelectedPackage == null || string.IsNullOrWhiteSpace(ApprovedBy))
                return;
            try
            {
                await ReleasePackageAsync();

                MessageBox.Show($"Package '{SelectedPackage.Name}' has been released.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                ApprovedBy = string.Empty;
                IsPackagePending = false;
            }            
            catch (Exception ex)
            {
                ApprovedBy = string.Empty;
                LoggerService.Error(this, ex.Message);
                MessageBox.Show($"Failed to release package: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        async Task ReleasePackageAsync()
        {

            using var http = new HttpClient { BaseAddress = new Uri("https://irpsystems.wats.com/") };
            var apiToken = "UmVzdFRva2VuOmN0M1Y0M3Y2SU5JMXdYUzc2MDNRYTB5SSVQQzhWMg==";
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", apiToken);
            http.DefaultRequestHeaders.Accept.Clear();
            http.DefaultRequestHeaders.Accept.ParseAdd("application/json");

            var url = $"api/Software/PackageStatus/{SelectedPackage.PackageId}?status=Released"; // e.g., Released / Pending / Revoked

            // Some servers require a content-type even for empty POSTs:
            using var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent("", Encoding.UTF8, "application/json")
            };

            using var resp = await http.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"PackageStatus failed: {(int)resp.StatusCode} {resp.ReasonPhrase}.");
            }
        }
        void UpdateCanRelease()
        {
            CanReleasePendingPackage =
                !string.IsNullOrWhiteSpace(ApprovedBy)
                && SelectedPackage != null
                && SelectedPackage.Status == (int)StatusEnum.Pending;
        }
        public RelayCommand ReleasePackageCommand { get; private set; }
        public RelayCommand BrowseMCUParametersJsonPathCommand { get; private set; }

		public event Action LoadDevicesContainer;
	}
}
