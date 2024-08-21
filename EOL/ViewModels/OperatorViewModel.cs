
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceHandler.Models;
using EOL.Models;
using EOL_Tester.Classes;
using ScriptHandler.Models;

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
			SettingsViewModel viewModel)
		{

			RunData = new RunData();
			Run = new RunViewModel(
				@"C:\Users\smadar\Documents\Scripts\Test scripts\EOL.scr", 
				devicesContainer, 
				RunData,
				userDefaultSettings, viewModel);
			RunData.RunScript = Run.RunScript;
		}		

		#endregion Constructor

		#region Methods

		

		#endregion Methods

		#region Commands

		

		#endregion Commands
	}
}
