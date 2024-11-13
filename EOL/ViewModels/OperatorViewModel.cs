
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceHandler.Models;
using EOL.Models;
using ScriptHandler.Models;
using Services.Services;
using System.Windows.Controls;

namespace EOL.ViewModels
{
	public class OperatorViewModel:ObservableObject
	{
		public enum ScriptStateEnum { Running, Pass, Fail, None, }

		#region Properties

		public RunData RunData { get; set; }
		public RunViewModel Run { get; set; }

		#endregion Properties

		#region Fields

		

		#endregion Fields

		#region Constructor

		public OperatorViewModel(
			DevicesContainer devicesContainer,
			ScriptUserData scriptUserData,
			UserDefaultSettings userDefaultSettings,
			SettingsViewModel viewModel, 
			RunData runData,
			RichTextBox richTextBox,
			LogLineListService logLineList)
		{
            RunData = runData;
            Run = new RunViewModel( 
				devicesContainer,
                RunData,
				userDefaultSettings, 
				viewModel,
				logLineList);
            RunData.RunScript = Run.RunScript;
		}

		#endregion Constructor

		#region Methods

		public void ChangeDarkLight(bool isLightTheme)
		{
			Run.ChangeDarkLight(isLightTheme);
		}

		#endregion Methods

		#region Commands



		#endregion Commands
	}
}
