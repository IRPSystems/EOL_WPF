
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EOL.Models;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EOL.ViewModels
{
    public class SettingsViewModel: ObservableObject
    {
        public ObservableCollection<FilesData> FilesList { get; set; }
		public double DescriptsionColumnWidth { get; set; }

        public SettingsViewModel()
        {
			BrowseFilePathCommand = new RelayCommand<FilesData>(BrowseFilePath);
			LoadedCommand = new RelayCommand(Loaded);
			
		}

		private void Loaded()
		{
			GetDescriptsionColumnWidth();
		}

		private void GetDescriptsionColumnWidth()
		{
			double maxWidth = 0;
			foreach (FilesData filesData in FilesList)
			{
				if (filesData == null || string.IsNullOrEmpty(filesData.Description))
					continue;

				TextBlock textBlock = new TextBlock();
				var formattedText = new FormattedText(
					filesData.Description,
					CultureInfo.CurrentCulture,
					FlowDirection.LeftToRight,
					new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch),
					textBlock.FontSize,
					Brushes.Black,
					new NumberSubstitution(),
					VisualTreeHelper.GetDpi(textBlock).PixelsPerDip);

				if (formattedText.Width > maxWidth)
					maxWidth = formattedText.Width;
			}

			DescriptsionColumnWidth = maxWidth + 10;
			if(DescriptsionColumnWidth > 200)
				DescriptsionColumnWidth = 200;
		}


		private void BrowseFilePath(FilesData filesData)
        {
			OpenFileDialog openFileDialog = new OpenFileDialog();			
			bool? result = openFileDialog.ShowDialog();
			if (result != true)
				return;

			filesData.Path = openFileDialog.FileName;
		}


		public RelayCommand<FilesData> BrowseFilePathCommand { get; private set; }
		public RelayCommand LoadedCommand { get; private set; }

	}
}
