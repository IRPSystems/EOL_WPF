using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EOL.Models;
using EOL.Models.Config;
using EOL.Views;
using Newtonsoft.Json;
using Serilog.Core;
using Services.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Virinco.Newtonsoft.Json.Linq;
using Virinco.WATS.Interface.MES.Software;
using Virinco.WATS.Service.MES.Contract;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;

namespace EOL.ViewModels
{
    public class SequenceFileInfo
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public SequenceFileInfo(string name, string fullPath)
        {
            Name = name;
            FullPath = fullPath;
        }
    }

    public class TaggedPackage
    {
        public Package Package { get; set; }
        public string StationType { get; set; }
        public string Project { get; set; }
        public PackageVersion VersionAndStatus { get; set; }
    }

    public class PackageVersion
    {
        public string Version { get; set; }
        public StatusEnum Status { get; set; }
        public string TesterAppVersion { get; set; }
    }
    public class WatsConfigSelectorViewModel : ObservableObject
    {
        public bool isContinue { get; private set; } = false;
        public Package SelectedPackage
        {
            get => _selectedPackage;
            set => SetProperty(ref _selectedPackage, value);
        }
        public ObservableCollection<TaggedPackage> DisplayPackages { get; } = new();
       
        public List<SequenceFileInfo> PackageTopLevelSequenceFiles { get; private set; } = new();
        public List<SequenceFileInfo> TesterAppTopLevelSequenceFiles { get; private set; } = new();

        public EOLSettings eolSettings;
        private readonly Virinco.WATS.Interface.MES.Software.Software _software;
        private Package _selectedPackage;
        public ObservableCollection<string> StationTypes { get; } = new();
        public ObservableCollection<string> AvailableProjects { get; } = new();
        public ObservableCollection<PackageVersion> AvailableVersions { get; } = new();

        private string _selectedStationType;
        public string SelectedStationType
        {
            get => _selectedStationType;
            set
            {
                if (_selectedStationType == value) return;
                _selectedStationType = value;
                OnPropertyChanged(nameof(SelectedStationType));
                RebuildProjects();     // cascade
                SelectedProject = null; // reset downstream
                AvailableVersions.Clear();
                SelectedPackageVersion = null;
                IsPackageVersionEnabled = false;
                UpdateReady();
            }
        }

        private string _selectedProject;
        public string SelectedProject
        {
            get => _selectedProject;
            set
            {
                if (_selectedProject == value) return;
                _selectedProject = value;
                OnPropertyChanged(nameof(SelectedProject));
                RebuildVersions();     // cascade
                SelectedPackageVersion = null;
                UpdateReady();
            }
        }

        private PackageVersion _selectedPackageVersion;
        public PackageVersion SelectedPackageVersion
        {
            get => _selectedPackageVersion;
            set
            {
                if (_selectedPackageVersion == value) return;
                _selectedPackageVersion = value;
                OnPropertyChanged(nameof(SelectedPackageVersion));
                UpdateReady();
            }
        }

        // Enable/Disable states
        private bool _isStationTypesEnabled = true;
        public bool IsStationTypesEnabled
        {
            get => _isStationTypesEnabled;
            set { if (_isStationTypesEnabled != value) { _isStationTypesEnabled = value; OnPropertyChanged(nameof(_isStationTypesEnabled)); } }
        }
        private bool _isProjectEnabled;
        public bool IsProjectEnabled
        {
            get => _isProjectEnabled;
            set { if (_isProjectEnabled != value) { _isProjectEnabled = value; OnPropertyChanged(nameof(IsProjectEnabled)); } }
        }

        private bool _isPackageVersionEnabled;
        public bool IsPackageVersionEnabled
        {
            get => _isPackageVersionEnabled;
            set { if (_isPackageVersionEnabled != value) { _isPackageVersionEnabled = value; OnPropertyChanged(nameof(IsPackageVersionEnabled)); } }
        }

        private bool _isReadyToConfirm;
        public bool IsReadyToConfirm
        {
            get => _isReadyToConfirm;
            set { if (_isReadyToConfirm != value) { _isReadyToConfirm = value; OnPropertyChanged(nameof(IsReadyToConfirm)); } }
        }

        private bool _isContinueEnabled;
        public bool IsContinueEnabled
        {
            get => _isContinueEnabled;
            set { if (_isContinueEnabled != value) { _isContinueEnabled = value; OnPropertyChanged(nameof(IsContinueEnabled)); } }
        }

        public WatsConfigSelectorViewModel( EOLSettings eolsettings)
        {
            eolSettings = eolsettings;
            _software = new Virinco.WATS.Interface.MES.Software.Software();
            ConfirmCommand = new RelayCommand(OnConfirm);
            ContinueCommand = new RelayCommand(Continue);
        }

        public void init()
        {
            LoadPackages();

            if (!string.IsNullOrEmpty(eolSettings.StationType) && eolSettings.PackageId != Guid.Empty)
            {
                var package = DisplayPackages.FirstOrDefault(p => p.Package.PackageId == eolSettings.PackageId);
                if (package!= null)
                {                  
                    SelectedStationType = eolSettings.StationType;
                    IsStationTypesEnabled = false;
                    SelectedProject = package.Project;
                    IsProjectEnabled = true;
                    SelectedPackageVersion = package.VersionAndStatus;
                    IsContinueEnabled = true;
                }
            }
        }

        public void LoadPackages()
        {
            try
            {
                //var packageavailable = 0;
                var ReleasedPackages = _software.GetPackages(Install: false);
                var PendingPackages = _software.GetPackages(Install: false,PackageStatus: StatusEnum.Pending);
                var allPackages = ReleasedPackages.Concat(PendingPackages).ToList();

                DisplayPackages.Clear();

                foreach (var pkg in allPackages)
                {
                    if (pkg.PackageTags != null)
                    {
                        var tagged = new TaggedPackage
                        {
                            Package = pkg,
                            StationType = GetTagValue(pkg, "StationType"),
                            Project = GetTagValue(pkg, "Project"),
                            VersionAndStatus = new PackageVersion
                            {
                                Version = GetTagValue(pkg, "Version"),
                                TesterAppVersion = GetTagValue(pkg, "TesterApp"),
                                Status = (StatusEnum)pkg.Status
                            }
                        };

                        DisplayPackages.Add(tagged);
                    }
                }
                RebuildStations();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading packages: " + ex.Message);
                LoggerService.Inforamtion(this, "Failed to load packages: " + ex.Message);
                return;
            }
        }

        private void OnConfirm()
        {
            TaggedPackage selectedtaggedpackage = new TaggedPackage();

            selectedtaggedpackage = DisplayPackages.FirstOrDefault(p =>
                p.StationType == SelectedStationType &&
                p.Project == SelectedProject &&
                p.VersionAndStatus == SelectedPackageVersion);

            SelectedPackage = selectedtaggedpackage?.Package;

            if (SelectedPackage != null)
            {
                if(SelectedPackage.Status == (int)StatusEnum.Pending)
                {
                    bool? result = PasswordRequested?.Invoke();
                    if (result != true)
                    {
                        return; // User cancelled or failed authentication
                    }
                }

                DownloadPackage(SelectedPackage);
                if(selectedtaggedpackage.VersionAndStatus.TesterAppVersion != null)
                { 
                    if(!selectedtaggedpackage.VersionAndStatus.TesterAppVersion.Contains(GetAppVersion()))
                        DownloadTesterApp(selectedtaggedpackage.VersionAndStatus.TesterAppVersion);
                }
                eolSettings.StationType = SelectedStationType;
                eolSettings.PackageId = SelectedPackage.PackageId;
                CloseEvent?.Invoke();
            }
        }

        private void Continue()
        {
            isContinue = true;
            SelectedPackage = DisplayPackages.FirstOrDefault(p => p.Package.PackageId == eolSettings.PackageId)?.Package;
            DownloadPackage(SelectedPackage);
            CloseEvent?.Invoke();
        }

        private void DownloadPackage(Package package)
        {
            try
            {
                List<FileInfo> executeFiles;
                List<FileInfo> topLevelSequences;


                _software.InstallPackage(
                    package,
                    DisplayProgress: true,     // Let WATS show the built-in progress window
                    WaitForExecution: true,    // Wait for installation to complete
                    out executeFiles,
                    out topLevelSequences
                );

                PackageTopLevelSequenceFiles = topLevelSequences
                 .Select(f => new SequenceFileInfo(f.Name, f.FullName))
                 .ToList();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Installation failed: {ex.Message}");
                // Optionally display an error dialog to the user
            }
        }

        private void DownloadTesterApp(string testerversion)
        {
            try
            {
                List<FileInfo> executeFiles;
                List<FileInfo> topLevelSequences;


                _software.InstallPackage(
                    package,
                    DisplayProgress: true,     // Let WATS show the built-in progress window
                    WaitForExecution: true,    // Wait for installation to complete
                    out executeFiles,
                    out topLevelSequences
                );

                TesterAppTopLevelSequenceFiles = topLevelSequences
                 .Select(f => new SequenceFileInfo(f.Name, f.FullName))
                 .ToList();



            }
            catch (Exception ex)
            {
                MessageBox.Show($"Installation failed: {ex.Message}");
                // Optionally display an error dialog to the user
            }
        }

        public bool LoadToUserSettings()
        {
            try
            {
                SetScriptPath("Abort Script.scr", path => eolSettings.UserDefaultSettings.DefaultAbortScriptFile = path);
                SetScriptPath(".gprj", path => eolSettings.UserDefaultSettings.DefaultMainSeqConfigFile = path);
                SetScriptPath("SafetyOfficer", path => eolSettings.UserDefaultSettings.DefaultSafetyScriptFile = path);
                SetScriptPath("merge_app", path => eolSettings.UserDefaultSettings.FirstFlashFilePath = path);
                SetScriptPath("merge_boot", path => eolSettings.UserDefaultSettings.SecondFlashFilePath = path);
                SetScriptPath("monitor", path => eolSettings.UserDefaultSettings.DefaultMonitorLogScript = path);
                SetScriptPath("param_defaults_all", path => eolSettings.MCUParametersJsonPath = path);

                ApplyConfigJsonOverrides();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load settings: {ex.Message}");
                return false;
            }
        }

        private void SetScriptPath(string fileName, Action<string> assignPath)
        {
            var file = PackageTopLevelSequenceFiles.FirstOrDefault(f =>
                f.Name.Contains(fileName, StringComparison.OrdinalIgnoreCase)); 
            
            if (file != null)
            {
                // Use Path.GetFullPath to normalize slashes if needed
                var cleanedPath = file.FullPath.Replace(@"\\", @"\");
                assignPath(cleanedPath);
            }
        }


        private string GetTagValue(Package package, string tagName)
        {
            return package.PackageTags?
                .FirstOrDefault(t => t.Name.Equals(tagName, StringComparison.OrdinalIgnoreCase))
                ?.Value ?? string.Empty;
        }

        private void RebuildStations()
        {
            StationTypes.Clear();
            foreach (var s in DisplayPackages
                .Select(p => p.StationType)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .OrderBy(s => s))
            {
                StationTypes.Add(s);
            }
            // Downstream disabled until a station is chosen
            IsProjectEnabled = false;
            IsPackageVersionEnabled = false;
        }

        private void RebuildProjects()
        {
            AvailableProjects.Clear();

            if (string.IsNullOrWhiteSpace(SelectedStationType))
            {
                IsProjectEnabled = false;
                return;
            }

            foreach (var p in DisplayPackages
                .Where(x => x.StationType == SelectedStationType)
                .Select(x => x.Project)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct()
                .OrderBy(p => p))
            {
                AvailableProjects.Add(p);
            }

            IsProjectEnabled = AvailableProjects.Any();
            IsPackageVersionEnabled = false;
        }

        private void RebuildVersions()
        {
            AvailableVersions.Clear();

            if (string.IsNullOrWhiteSpace(SelectedStationType) || string.IsNullOrWhiteSpace(SelectedProject))
            {
                IsPackageVersionEnabled = false;
                return;
            }

            foreach (var v in DisplayPackages
                .Where(x => x.StationType == SelectedStationType && x.Project == SelectedProject)
                .Select(x => x.VersionAndStatus)
                .Where(v => !string.IsNullOrWhiteSpace(v.Version))
                .Distinct()
                .OrderByDescending(v => v.Version,StringComparer.OrdinalIgnoreCase))
            {
                AvailableVersions.Add(v);
            }

            IsPackageVersionEnabled = AvailableVersions.Any();
        }

        private void UpdateReady()
        {
            IsReadyToConfirm = !string.IsNullOrWhiteSpace(SelectedStationType)
                               && !string.IsNullOrWhiteSpace(SelectedProject)
                               && SelectedPackageVersion != null;
        }

        private void ApplyConfigJsonOverrides()
        {
            var files = PackageTopLevelSequenceFiles;
            if (files == null) return;

            // Try to get a full path to config.json (case-insensitive)
            string configPath = files.FirstOrDefault(p => p.Name.Equals("config.json", StringComparison.OrdinalIgnoreCase))?.FullPath ?? string.Empty;

            if (string.IsNullOrWhiteSpace(configPath) || !File.Exists(configPath))
                return;

            var json = File.ReadAllText(configPath);

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,      // don't overwrite with nulls
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            // json = contents of config.json
            JsonConvert.PopulateObject(json, eolSettings, settings);

        }

        public Package GetSelectedPackageId()
        {
            return SelectedPackage ?? null ;
        }

        private string GetAppVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";
        }

        public event Action CloseEvent;
        public event Func<bool> PasswordRequested;
        public RelayCommand ConfirmCommand { get; }
        public RelayCommand ContinueCommand { get; }

    }

    
}