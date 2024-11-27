
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceHandler.Models;
using EOL.Models;
using Microsoft.Win32;
using System;
using System.IO;

namespace EOL.ViewModels
{
	public class SettingsAdminViewModel: ObservableObject
	{


		public EOLSettings EolSettings { get; set; }

		public SettingsAdminViewModel(
			EOLSettings eolSettings)
		{
			EolSettings = eolSettings;
			BrowseMCUParametersJsonPathCommand = new RelayCommand(BrowseMCUParametersJsonPath);
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

		public RelayCommand BrowseMCUParametersJsonPathCommand { get; private set; }

		public event Action LoadDevicesContainer;
	}
}
