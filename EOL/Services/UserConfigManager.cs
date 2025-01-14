﻿using EOL.Models;
using EOL.Models.Config;
using EOL.ViewModels;
using EOL.Views;
using Newtonsoft.Json;
using Services.Services;
using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace EOL.Services
{
    public class UserConfigManager
    {
        private readonly string ConfigFilePath;
        private readonly string TechLogDirectory = "\\Technician Logs";
        private string ErrorMsg = "Missing file in subfolders:\r\n";

        public UserConfigManager()
        {
			string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            path = Path.Combine(path, "EOL");
			ConfigFilePath = Path.Combine(path, "user.config");
		}

		public bool? ReadConfig(EOLSettings eolSettings)
        {
			bool? result = true;

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

                    // Use reflection to assign the value to the corresponding property in UserDefaultSettings
                    var property = typeof(UserDefaultSettings).GetProperty(key, BindingFlags.Public | BindingFlags.Instance);
                    if (property != null && value != null)
                    {
                        try
                        {
                            // Convert value to the correct type and set the property value
                            object convertedValue = Convert.ChangeType(value, property.PropertyType);
                            property.SetValue(eolSettings.UserDefaultSettings, convertedValue);
                        }
                        catch (Exception ex)
                        {
                            // Log or handle the exception
                            Console.WriteLine($"Error setting property '{key}': {ex.Message}");
                        }
                    }
                }

                if (!string.IsNullOrEmpty(eolSettings.UserDefaultSettings.ReportsSavingPath))
                {
                    eolSettings.UserDefaultSettings.TechLogDirectory = eolSettings.UserDefaultSettings.ReportsSavingPath + TechLogDirectory;
                }

                if (string.IsNullOrEmpty(eolSettings.UserDefaultSettings.AutoConfigPref))
                {
                    result = AutoConfigProcedure(eolSettings.UserDefaultSettings);
                }
            }
            else
            {
                AutoConfigProcedure(eolSettings.UserDefaultSettings);
            }

            return result;
        }

        private bool? AutoConfigProcedure(UserDefaultSettings userDefaultSettings)
        {
			bool? result = RunAutoConfigUI(userDefaultSettings);
            if (!string.IsNullOrEmpty(userDefaultSettings.AutoConfigPref))
            {
                TryAutoPackageConfig(userDefaultSettings);
            }

            return result;
        }

        private bool? RunAutoConfigUI(UserDefaultSettings userDefaultSettings)
        {
            ConfigPrefWinddowVIew configPrefWinddowVIew = new ConfigPrefWinddowVIew()
            {
                DataContext = new ConfigPrefVIewModel(userDefaultSettings),
            };

			bool? result = configPrefWinddowVIew.ShowDialog();
			SaveConfig(userDefaultSettings);

            return result;
		}

        private void TryAutoPackageConfig(UserDefaultSettings userDefaultSettings)
        {
            ResetPackageRelevantConfigs(userDefaultSettings);
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

                    config = packageConfig.GetConfiguration(userDefaultSettings.AutoConfigPref);

                    if (config.MainSeq.IsUsed)
                    {
                        SubFolderMainSeq = config.MainSeq.SubfolderName;
                        destFileExtension = config.MainSeq.FileExtension;
                        userDefaultSettings.DefaultMainSeqConfigFile = SearchPath(parentDirectoryPath, SubFolderMainSeq, destFileExtension);
                        userDefaultSettings.UseDefaultMainSeqConfigFile = true;
                    }

                    if (config.ProjectSeq.IsUsed)
                    {
                        SubFolderMainSeq = config.ProjectSeq.SubfolderName;
                        destFileExtension = config.ProjectSeq.FileExtension;
                        userDefaultSettings.UseDefaultSubscriptFile = true;
                        userDefaultSettings.DefaultSubScriptFile = SearchPath(parentDirectoryPath, SubFolderMainSeq, destFileExtension);
                    }

                    if (config.MonitorLog.IsUsed)
                    {
                        SubFolderMainSeq = config.MonitorLog.SubfolderName;
                        destFileExtension = config.MonitorLog.FileExtension;
                        userDefaultSettings.UseDefaultMonitorLogFile = true;
                        userDefaultSettings.DefaultMonitorLogScript = SearchPath(parentDirectoryPath, SubFolderMainSeq, destFileExtension);
                    }

                    if (config.FirstFlashFile.IsUsed)
                    {
                        SubFolderMainSeq = config.FirstFlashFile.SubfolderName;
                        destFileExtension = config.FirstFlashFile.FileExtension;
                        userDefaultSettings.UseDefaultFirstFlashFile = true;
                        userDefaultSettings.FirstFlashFilePath = SearchPath(parentDirectoryPath, SubFolderMainSeq, destFileExtension);
                    }

                    if (config.SecondFlashFile.IsUsed)
                    {
                        SubFolderMainSeq = config.SecondFlashFile.SubfolderName;
                        destFileExtension = config.SecondFlashFile.FileExtension;
                        userDefaultSettings.UseDefaultSecondFlashFile = true;
                        userDefaultSettings.SecondFlashFilePath = SearchPath(parentDirectoryPath, SubFolderMainSeq);
                    }
                    
                    userDefaultSettings.isRecordMonitor = config.OtherPreferences.MonitorLog;
                    userDefaultSettings.isPrintLabel = config.OtherPreferences.PrintLabel;
                    userDefaultSettings.ProjectBaudRate = config.OtherPreferences.ProjectBaudRate;

                    userDefaultSettings.preTestFlash = config.FlashPreferences.PreRunFlash;
                    userDefaultSettings.isTogglePower = config.FlashPreferences.IsTogglePower;
                    userDefaultSettings.FlashPowerEA_PS = config.FlashPreferences.PsFlashPower;
                    userDefaultSettings.FlashPowerUsbRelay = config.FlashPreferences.RelayFlashPower;
                    userDefaultSettings.FlashPowerATEBox = config.FlashPreferences.AteBoxFlashPower;

                    if (config.FlashPreferences.FirstFileArguments != null)
                    {
                        userDefaultSettings.FirstFlashFileCustomerProp = config.FlashPreferences.FirstFileArguments.Customer;
                    }

                    if (config.FlashPreferences.SecondFileArguments != null)
                    {
                        userDefaultSettings.SecondFlashFileCustomerProp = config.FlashPreferences.SecondFileArguments.Customer;
                    }

					if (config.DevicesList != null)
					{
						userDefaultSettings.MCU = config.DevicesList.MCU;
						userDefaultSettings.MCU_2 = config.DevicesList.MCU_2;
						userDefaultSettings.ZimmerPowerMeter = config.DevicesList.ZimmerPowerMeter;
						userDefaultSettings.NI_6002 = config.DevicesList.NI_6002;
						userDefaultSettings.NI_6002_2 = config.DevicesList.NI_6002_2;
						userDefaultSettings.Printer_TSC = config.DevicesList.Printer_TSC;
						userDefaultSettings.NumatoGPIO = config.DevicesList.NumatoGPIO;
						userDefaultSettings.PowerSupplyEA = config.DevicesList.PowerSupplyEA;
					}

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


        private void ResetPackageRelevantConfigs(UserDefaultSettings userDefaultSettings)
        {
			userDefaultSettings.UseDefaultMainSeqConfigFile = false;
			userDefaultSettings.UseDefaultMonitorLogFile = false;
            userDefaultSettings.UseDefaultSubscriptFile = false;
            userDefaultSettings.UseDefaultSecondFlashFile = false;
            userDefaultSettings.UseDefaultFirstFlashFile = false;
            userDefaultSettings.DefaultMainSeqConfigFile = null;
            userDefaultSettings.DefaultSubScriptFile = null;
            userDefaultSettings.DefaultMonitorLogScript = null;
            userDefaultSettings.FirstFlashFilePath = null;
            userDefaultSettings.SecondFlashFilePath = null;
            userDefaultSettings.isRecordMonitor = false;
            userDefaultSettings.preTestFlash = false;
            userDefaultSettings.FlashPowerATEBox = false;
            userDefaultSettings.FlashPowerEA_PS = false;
            userDefaultSettings.FlashPowerUsbRelay = false;
            userDefaultSettings.isPrintLabel = false;
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

        public void SaveConfig(UserDefaultSettings userDefaultSettings)
        {
            try
            {
                ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap
                {
                    ExeConfigFilename = ConfigFilePath
                };

                Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

                var appSettings = config.AppSettings.Settings;

                foreach (PropertyInfo property in typeof(UserDefaultSettings).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    string key = property.Name;
                    object value = property.GetValue(userDefaultSettings);
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

                if (!string.IsNullOrEmpty(userDefaultSettings.ReportsSavingPath))
                {
                    userDefaultSettings.TechLogDirectory = userDefaultSettings.ReportsSavingPath + TechLogDirectory;
                }

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
            }
            catch(Exception ex) 
            {
                LoggerService.Error(this, "Filed to save the configuration", "Error", ex);
            }
        }
    }
}
