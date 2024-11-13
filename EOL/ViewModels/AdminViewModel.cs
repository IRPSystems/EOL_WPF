
using CommunityToolkit.Mvvm.ComponentModel;
using Org.BouncyCastle.Ocsp;
using ScriptHandler.Models;
using ScriptHandler.ViewModels;
using ScriptRunner.Services;
using ScriptRunner.ViewModels;
using System.Collections.ObjectModel;

namespace EOL.ViewModels
{
	public class AdminViewModel: ObservableObject
	{
		public ScriptDiagramViewModel ScriptDiagram { get; set; }
		public ScriptLoggerService MainScriptLogger { get; set; }
		public RunExplorerViewModel RunExplorer { get; set; }

		public AdminViewModel(
			ScriptDiagramViewModel scriptDiagram,
			ScriptLoggerService mainScriptLogger,
			ObservableCollection<GeneratedProjectData> generatedProjectsList) 
		{
			ScriptDiagram = scriptDiagram;
			MainScriptLogger = mainScriptLogger;

			RunExplorer = new RunExplorerViewModel(generatedProjectsList);
			RunExplorer.TestDoubleClickedEvent += RunExplorer_TestDoubleClickedEvent;
		}

		private void RunExplorer_TestDoubleClickedEvent(GeneratedTestData testData)
		{
			ScriptDiagram.DrawScript(testData);
		}
	}
}
