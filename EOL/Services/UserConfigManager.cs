using EOL.ViewModels;
using EOL.Views;
using EOL_Tester.Classes;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace EOL.Services
{
    public class UserConfigManager
    {
        private readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "user.config");
        private readonly string TechLogDirectory = "\\Technician Logs";
        private string ErrorMsg = "Missing file in subfolders:\r\n";

        public void ReadConfig()
        {
            if (File.Exists(ConfigFilePath))
            {
                ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap
                {
                    ExeConfigFilename = ConfigFilePath
                };

                Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

                var appSettings = config.AppSettings.Settings;

                foreach (string key in appSettings.AllKeys)
                {
                    string value = appSettings[key]?.Value;

                    // Use reflection to assign the value to the corresponding variable in UserDefaultSettings
                    var variable = typeof(UserDefaultSettings).GetProperty(key, BindingFlags.Public | BindingFlags.Static);
                    if (variable != null && value != null)
                    {
                        variable.SetValue(null, Convert.ChangeType(value, variable.PropertyType));
                    }
                }

                if (!string.IsNullOrEmpty(UserDefaultSettings.ReportsSavingPath))
                {
                    UserDefaultSettings.TechLogDirectory = UserDefaultSettings.ReportsSavingPath + TechLogDirectory;
                }

                if (string.IsNullOrEmpty(UserDefaultSettings.AutoConfigPref))
                {
                    AutoConfigProcedure();
                }
            }
            else
            {
                AutoConfigProcedure();
            }
        }

        private void AutoConfigProcedure()
        {
            RunAutoConfigUI();
            if (!string.IsNullOrEmpty(UserDefaultSettings.AutoConfigPref))
            {
                TryAutoPackageConfig();
            }
        }

        private void RunAutoConfigUI()
        {
            ConfigPrefWinddowVIew configPrefWinddowVIew = new ConfigPrefWinddowVIew()
            {
                DataContext = new ConfigPrefVIewModel(),
            };

			configPrefWinddowVIew.ShowDialog();
			SaveConfig();
		}

        private void TryAutoPackageConfig()
        {
            ResetPackageRelevantConfigs();
            // Get the current directory
            string currentDirectory = Directory.GetCurrentDirectory();

            // Create a DirectoryInfo object
            DirectoryInfo directoryInfo = new DirectoryInfo(currentDirectory);

            // Get the parent directory
            DirectoryInfo parentDirectory = directoryInfo.Parent;

            if (!Validate(parentDirectory))
            {
                return;
            }

            string fileName = "PackageConfigs.json";
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    PackageConfig packageConfig = JsonConvert.DeserializeObject<PackageConfig>(json);
                    string parentDirectoryPath = parentDirectory.FullName;

                    //test
                    //string parentDirectoryPath = "C:\\dev\\TesterPackage";

                    string SubFolderMainSeq;
                    string destFileExtension;

                    Config config;

                    config = packageConfig.GetConfiguration(UserDefaultSettings.AutoConfigPref);

                    if (config.MainSeq.IsUsed)
                    {
                        SubFolderMainSeq = config.MainSeq.SubfolderName;
                        destFileExtension = config.MainSeq.FileExtension;
                        UserDefaultSettings.DefaultMainSeqConfigFile = SearchPath(parentDirectoryPath, SubFolderMainSeq, destFileExtension);
                        UserDefaultSettings.UseDefaultMainSeqConfigFile = true;
                    }

                    if (config.ProjectSeq.IsUsed)
                    {
                        SubFolderMainSeq = config.ProjectSeq.SubfolderName;
                        destFileExtension = config.ProjectSeq.FileExtension;
                        UserDefaultSettings.UseDefaultSubscriptFile = true;
                        UserDefaultSettings.DefaultSubscriptFile = SearchPath(parentDirectoryPath, SubFolderMainSeq, destFileExtension);
                    }

                    if (config.MonitorLog.IsUsed)
                    {
                        SubFolderMainSeq = config.MonitorLog.SubfolderName;
                        destFileExtension = config.MonitorLog.FileExtension;
                        UserDefaultSettings.UseDefaultMonitorLogFile = true;
                        UserDefaultSettings.DefaultMonitorLogFile = SearchPath(parentDirectoryPath, SubFolderMainSeq, destFileExtension);
                    }

                    if (config.FirstFlashFile.IsUsed)
                    {
                        SubFolderMainSeq = config.FirstFlashFile.SubfolderName;
                        destFileExtension = config.FirstFlashFile.FileExtension;
                        UserDefaultSettings.UseDefaultFirstFlashFile = true;
                        UserDefaultSettings.FirstFlashFilePath = SearchPath(parentDirectoryPath, SubFolderMainSeq, destFileExtension);
                    }

                    if (config.SecondFlashFile.IsUsed)
                    {
                        SubFolderMainSeq = config.SecondFlashFile.SubfolderName;
                        destFileExtension = config.SecondFlashFile.FileExtension;
                        UserDefaultSettings.UseDefaultSecondFlashFile = true;
                        UserDefaultSettings.SecondFlashFilePath = SearchPath(parentDirectoryPath, SubFolderMainSeq);
                    }


                    UserDefaultSettings.preTestFlash = config.OtherPreferences.PreFlash;
                    UserDefaultSettings.isUseMonitorLog = config.OtherPreferences.MonitorLog;
                    UserDefaultSettings.isPrintLabel = config.OtherPreferences.PrintLabel;
                    UserDefaultSettings.isTogglePower = config.OtherPreferences.IsTogglePower;
                    UserDefaultSettings.FlashPowerEA_PS = config.OtherPreferences.PsFlashPower;
                    UserDefaultSettings.FlashPowerUsbRelay = config.OtherPreferences.RelayFlashPower;
                    UserDefaultSettings.FlashPowerATEBox = config.OtherPreferences.AteBoxFlashPower;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while parsing the JSON file: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"The file '{fileName}' was not found in the current directory.");
            }
        }


        private void ResetPackageRelevantConfigs()
        {
            UserDefaultSettings.UseDefaultMainSeqConfigFile = false;
            UserDefaultSettings.UseDefaultMonitorLogFile = false;
            UserDefaultSettings.UseDefaultSubscriptFile = false;
            UserDefaultSettings.UseDefaultSecondFlashFile = false;
            UserDefaultSettings.UseDefaultFirstFlashFile = false;
            UserDefaultSettings.DefaultMainSeqConfigFile = null;
            UserDefaultSettings.DefaultSubscriptFile = null;
            UserDefaultSettings.DefaultMonitorLogFile = null;
            UserDefaultSettings.FirstFlashFilePath = null;
            UserDefaultSettings.SecondFlashFilePath = null;
            UserDefaultSettings.isUseMonitorLog = false;
            UserDefaultSettings.preTestFlash = false;
            UserDefaultSettings.FlashPowerATEBox = false;
            UserDefaultSettings.FlashPowerEA_PS = false;
            UserDefaultSettings.FlashPowerUsbRelay = false;
            UserDefaultSettings.isPrintLabel = false;
        }

        private string SearchPath(string parentDirectory, string subFolderMainSeq, string destFileExtension = null)
        {

            try
            {
                string[] files;

                string[] subFolders = subFolderMainSeq.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

                string directoryToSearch = parentDirectory;
                foreach (string folder in subFolders)
                {
                    directoryToSearch = Path.Combine(directoryToSearch, folder);
                }
                // Get all files with the specified extension in the given path
                if (destFileExtension != null)
                {
                    files = Directory.GetFiles(directoryToSearch, destFileExtension);
                }
                else
                {
                    files = Directory.GetFiles(directoryToSearch);
                }

                // Check if any files were found
                if (files.Length == 0)
                {
                    ErrorMsg += subFolderMainSeq + ": " + destFileExtension;
                }
                else
                {
                    if (files.Length > 1)
                    {
                        //report to user that there are more than one file so no assign
                    }
                    else
                    {
                        return files[0];
                    }
                }
            }
            catch (Exception e)
            {
                // Handle any errors that might occur during the file search
                Console.WriteLine($"An error occurred: {e.Message}");
            }
            return null;
        }

        public bool Validate(DirectoryInfo parentDirectory)
        {
            // Check if the parent directory is not null
            if (parentDirectory != null)
            {
                // Get the full path of the parent directory
                return true;
            }
            else
            {
                Console.WriteLine("No parent directory found.");
                return false;
            }
        }

        public void SaveConfig()
        {
            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = ConfigFilePath
            };

            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

            var appSettings = config.AppSettings.Settings;

            foreach (PropertyInfo property in typeof(UserDefaultSettings).GetProperties(BindingFlags.Public | BindingFlags.Static))
            {
                string key = property.Name;
                object value = property.GetValue(null);
                if (value != null)
                {
                    string valueString = Convert.ToString(value);
                    if (appSettings[key] != null)
                    {
                        appSettings[key].Value = valueString;
                    }
                    else
                    {
                        appSettings.Add(key, valueString);
                    }
                }
            }

            if (!string.IsNullOrEmpty(UserDefaultSettings.ReportsSavingPath))
            {
                UserDefaultSettings.TechLogDirectory = UserDefaultSettings.ReportsSavingPath + TechLogDirectory;
            }

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
        }
    }
}
