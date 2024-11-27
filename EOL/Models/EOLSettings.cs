
using System.IO;
using System;
using Newtonsoft.Json;
using ScriptHandler.Models;
using DeviceHandler.Models;

namespace EOL.Models
{
	public class EOLSettings
	{
		public bool IsLightTheme { get; set; }
		public ScriptUserData ScriptUserData { get; set; }

		public DeviceSetupUserData DeviceSetupUserData { get; set; }

		public UserDefaultSettings UserDefaultSettings { get; set; }

		public SettingsData GeneralData { get; set; }

		public string MCUParametersJsonPath { get; set; }

		public EOLSettings()
		{
			IsLightTheme = false;
		}

		private static EOLSettings GetDefaultSettings()
		{
			EOLSettings eolSettings = new EOLSettings();
			eolSettings.IsLightTheme = false;
			eolSettings.ScriptUserData = new ScriptUserData();
			eolSettings.DeviceSetupUserData = new DeviceSetupUserData();
			eolSettings.UserDefaultSettings = new UserDefaultSettings();


			return eolSettings;
		}

		public static EOLSettings LoadEOLUserData(string dirName)
		{
			EOLSettings eolSettings = null;

			string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			path = Path.Combine(path, dirName);
			if (Directory.Exists(path) == false)
			{
				eolSettings = GetDefaultSettings();
				return eolSettings;
			}
			path = Path.Combine(path, "EOLSettings.json");
			if (File.Exists(path) == false)
			{
				eolSettings = GetDefaultSettings();
				return eolSettings;
			}


			string jsonString = File.ReadAllText(path);
			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;
			settings.TypeNameHandling = TypeNameHandling.All;
			eolSettings = JsonConvert.DeserializeObject(jsonString, settings) as EOLSettings;

			if (eolSettings.ScriptUserData == null)
				eolSettings.ScriptUserData = new ScriptUserData();

			if (eolSettings.DeviceSetupUserData == null)
				eolSettings.DeviceSetupUserData = new DeviceSetupUserData();

			if (eolSettings.UserDefaultSettings == null)
				eolSettings.UserDefaultSettings = new UserDefaultSettings();

			if (eolSettings != null)
				return eolSettings;

			eolSettings = GetDefaultSettings();

			return eolSettings;
		}



		public static void SaveEOLUserData(
			string dirName,
			EOLSettings eolSettings)
		{
			string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			path = Path.Combine(path, dirName);
			if (Directory.Exists(path) == false)
				return;
			path = Path.Combine(path, "EOLSettings.json");

			eolSettings.IsLightTheme = !eolSettings.IsLightTheme;

			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;
			settings.TypeNameHandling = TypeNameHandling.All;
			var sz = JsonConvert.SerializeObject(eolSettings, settings);
			File.WriteAllText(path, sz);
		}
	}
}
