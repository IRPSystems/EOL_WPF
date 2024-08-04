
using CommunityToolkit.Mvvm.ComponentModel;

namespace EOL.Models
{
    public class FilesData: ObservableObject
    {
        public string Description { get; set; }
		public string Path { get; set; }
	}
}
