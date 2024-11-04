
using CommunityToolkit.Mvvm.ComponentModel;
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
		public TestStudioLoggerService MainScriptLogger { get; set; }
		public RunExplorerViewModel RunExplorer { get; set; }

		public AdminViewModel(
			ScriptDiagramViewModel scriptDiagram,
			TestStudioLoggerService mainScriptLogger,
			ObservableCollection<GeneratedProjectData> generatedProjectsList) 
		{
			ScriptDiagram = scriptDiagram;
			MainScriptLogger = mainScriptLogger;

			RunExplorer = new RunExplorerViewModel(generatedProjectsList);
			//generatedProjectsList.CollectionChanged += GeneratedProjectsList_CollectionChanged;

		}

		//private void GeneratedProjectsList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		//{
		//	OnPropertyChanged(nameof(RunExplorer.ProjectsList));
		//}
	}
}
