using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System;
using Virinco.WATS.Interface.MES;
using Virinco.WATS.Service.MES.Contract;
using Virinco.WATS.Interface.MES.Software;
using Virinco.WATS.Interface;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using EOL.Models;

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
        public string StationName { get; set; }
        public string Project { get; set; }
        public string SwVersion { get; set; }
    }

    public partial class WatsConfigSelectorViewModel : ObservableObject
    {
        public bool isContinue { get; private set; } = false;
        public ObservableCollection<Package> Packages { get; } = new();
        public Package SelectedPackage
        {
            get => _selectedPackage;
            set => SetProperty(ref _selectedPackage, value);
        }
        public ObservableCollection<TaggedPackage> DisplayPackages { get; } = new();
       
        public string SelectedPackagePath { get; set; }
        public List<SequenceFileInfo> TopLevelSequenceFiles { get; private set; } = new();

        public RelayCommand ConfirmCommand { get; }
        public RelayCommand ContinueCommand { get; }

        public UserDefaultSettings userDefaultSettings;
        private readonly MesBase _mesBase;
        private readonly Software _software;
        private readonly TDM _tdm;
        private Package _selectedPackage;
        private string _selectedStationType;
        private string _selectedProject;
        private string _selectedSwVersion;



        public event Action CloseEvent;


        public WatsConfigSelectorViewModel(TDM tdm , UserDefaultSettings userDefault)
        {
            userDefaultSettings = userDefault;
            _tdm = tdm;
            _mesBase = new MesBase();
            _software = new Software();
            ConfirmCommand = new RelayCommand(OnConfirm);
            ContinueCommand = new RelayCommand(Continue);
            LoadPackages();
        }

        public void LoadPackages()
        {
            try
            {
                var result = _software.GetPackages();

                DisplayPackages.Clear();

                foreach (var pkg in result)
                {
                    var tagged = new TaggedPackage
                    {
                        Package = pkg,
                        StationName = GetTagValue(pkg, "StationName"),
                        Project = GetTagValue(pkg, "Project"),
                        SwVersion = GetTagValue(pkg, "SwVersion")
                    };

                    DisplayPackages.Add(tagged);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading packages: " + ex.Message);
            }
        }

        private void OnConfirm()
        {
            if (SelectedPackage != null)
            {
                _ = DownloadPackageAsync(SelectedPackage);
                CloseEvent?.Invoke();
            }
        }

        private void Continue()
        {
            isContinue = true;
            CloseEvent?.Invoke();
        }

        private async Task DownloadPackageAsync(Package package)
        {
            try
            {
                List<FileInfo> executeFiles;
                List<FileInfo> topLevelSequences;

                await Task.Run(() =>
                {
                    _software.InstallPackage(
                        package,
                        DisplayProgress: true,     // Let WATS show the built-in progress window
                        WaitForExecution: true,    // Wait for installation to complete
                        out executeFiles,
                        out topLevelSequences
                    );
                    string basePath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "EOL"
                    );
                    SelectedPackagePath = Path.Combine(basePath, package.Name);
                    TopLevelSequenceFiles = topLevelSequences
                     .Select(f => new SequenceFileInfo(f.Name, f.FullName))
                     .ToList();
                    //SaveInstalledFiles(executeFiles, topLevelSequences, SelectedPackagePath);
                    LoadToUserSettings();

                });

                CloseEvent?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Installation failed: {ex.Message}");
                // Optionally display an error dialog to the user
            }
        }

        private void SaveInstalledFiles(IEnumerable<FileInfo> executeFiles, IEnumerable<FileInfo> topLevelSequences, string targetDirectory)
        {
            try
            {
                if (!Directory.Exists(targetDirectory))
                {
                    object dir = Directory.CreateDirectory(targetDirectory);
                }

                // Combine and copy both file sets
                var allFiles = executeFiles.Concat(topLevelSequences).Distinct();

                foreach (var file in allFiles)
                {
                    var destPath = Path.Combine(targetDirectory, file.Name);

                    // Overwrite if already exists
                    File.Copy(file.FullName, destPath, overwrite: true);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save installed files: {ex.Message}");
            }
        }

        private void LoadToUserSettings()
        {
            try
            {
                SetScriptPath("Abort Script.scr", path => userDefaultSettings.DefaultAbortScriptFile = path);
                SetScriptPath("Hi Dynamic EOL.gprj", path => userDefaultSettings.DefaultMainSeqConfigFile = path);
                SetScriptPath("SafetyOfficer.scr", path => userDefaultSettings.DefaultSafetyScriptFile = path);
                SetScriptPath("merge_app", path => userDefaultSettings.FirstFlashFilePath = path);
                SetScriptPath("merge_boot", path => userDefaultSettings.SecondFlashFilePath = path);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load settings: {ex.Message}");
            }
        }

        private void SetScriptPath(string fileName, Action<string> assignPath)
        {
            var file = TopLevelSequenceFiles.FirstOrDefault(f =>
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
        //private async Task DownloadPackageAsync(Package package, IProgress<int> progress)
        //{
        //    // Simulate download with progress reporting
        //    for (int i = 0; i <= 100; i += 10)
        //    {
        //        await Task.Delay(100); // Replace with real download logic
        //        progress.Report(i);
        //    }
        //    // TODO: Replace with actual download logic and progress reporting

        //}
    }
}