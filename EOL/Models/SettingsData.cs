
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace EOL.Models
{
	public class SettingsData: ObservableObject
	{
		public ObservableCollection<FilesData> FilesList { get; set; }

		public bool IsIgnorFail { get; set; }
		public bool IsPrintLabel { get; set; }
        private string _rackNumber;
        public string RackNumber
        {
            get => _rackNumber;
            set
            {
                if (_rackNumber != value)
                {
                    _rackNumber = value;
                    OnPropertyChanged(nameof(RackNumber));
                }
            }
        }

        public bool IsPSoCPreferences { get; set; }
        public string PSoCPort1 { get; set; }
		public string PSoCPort2 { get; set; }

	}
}
