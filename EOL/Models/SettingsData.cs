
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace EOL.Models
{
	public class SettingsData: ObservableObject
	{
		public ObservableCollection<FilesData> FilesList { get; set; }

		public bool IsIgnorFail { get; set; }
		public bool IsPrintLabel { get; set; }
	}
}
