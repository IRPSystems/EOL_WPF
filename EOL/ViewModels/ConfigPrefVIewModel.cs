
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EOL_Tester.Classes;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EOL.ViewModels
{
	public class ConfigPrefVIewModel: ObservableObject
	{
		public ObservableCollection<string> ButtonsList { get; set; }
		public int WindowHeight { get; set; }
		public int ButtonHeight { get; set; }

		public ConfigPrefVIewModel() 
		{
			ButtonHeight = 35;

			Button_ClickedCommand = new RelayCommand<string>(Button_Clicked);

			OpenPackageConfigJSON();
		}

		private void OpenPackageConfigJSON()
		{
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
			ButtonsList = new ObservableCollection<string>();
			if (File.Exists(filePath))
			{
				try
				{
					string json = File.ReadAllText(filePath);
					PackageConfig packageConfig = JsonConvert.DeserializeObject<PackageConfig>(json);
					string parentDirectoryPath = parentDirectory.FullName;

					var configNames = packageConfig.Configurations.Keys.ToList();

					if (configNames.Any())
					{
						ButtonsList.Add(configNames[0]);
						if (configNames.Count > 1)
						{
							ButtonsList.Add(configNames[1]);
						}
						if (configNames.Count > 2)
						{
							ButtonsList.Add(configNames[2]);
						}
					}

					//for (int i = 0; i < ButtonsList.Count(); i++)
					//{
					//	if (String.IsNullOrEmpty(ButtonsList[i]))
					//	{
					//		buttons[i].BackColor = Color.Snow;
					//		buttons[i].Text = "Custom";
					//		break;
					//	}
					//}

					//UpdateButtons(configNames.Count + 1); //+1 for custom
					
					int buttonHeight = ButtonHeight + 4;
					WindowHeight = ButtonsList.Count * buttonHeight + 40;

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

		public static bool Validate(DirectoryInfo parentDirectory)
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

		private void Button_Clicked(string name)
		{
			UserDefaultSettings.AutoConfigPref = name;
			CloseEvent?.Invoke();
		}


		public RelayCommand<string> Button_ClickedCommand { get; private set; }

		public event Action CloseEvent;
	}
}
