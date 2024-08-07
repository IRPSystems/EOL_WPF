
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
		#region Properties

		public SettingsData SettingsData { get; set; }
		public double DescriptsionColumnWidth { get; set; }

		#endregion Properties

		#region Constructor

		public SettingsViewModel(SettingsData settingsData)
        {
			SettingsData = settingsData;

			BrowseFilePathCommand = new RelayCommand<FilesData>(BrowseFilePath);
			LoadedCommand = new RelayCommand(Loaded);
			
		}

		#endregion Constructor

		#region Methods

		private void Loaded()
		{
			GetDescriptsionColumnWidth();
		}

		private void GetDescriptsionColumnWidth()
		{
			double maxWidth = 0;
			foreach (FilesData filesData in SettingsData.FilesList)
			{
				if (filesData == null || string.IsNullOrEmpty(filesData.Description))
					continue;

				TextBlock textBlock = new TextBlock();
				var formattedText = new FormattedText(
					filesData.Description,
					CultureInfo.CurrentCulture,
					FlowDirection.LeftToRight,
					new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch),
					14,
					Brushes.Black,
					new NumberSubstitution(),
					VisualTreeHelper.GetDpi(textBlock).PixelsPerDip);

				if (formattedText.Width > maxWidth)
					maxWidth = formattedText.Width;
			}

			DescriptsionColumnWidth = maxWidth + 10;
			if(DescriptsionColumnWidth > 250)
				DescriptsionColumnWidth = 250;
		}


		private void BrowseFilePath(FilesData filesData)
        {
			OpenFileDialog openFileDialog = new OpenFileDialog();			
			bool? result = openFileDialog.ShowDialog();
			if (result != true)
				return;

			filesData.Path = openFileDialog.FileName;
		}

		#endregion Methods

		#region Commands

		public RelayCommand<FilesData> BrowseFilePathCommand { get; private set; }
		public RelayCommand LoadedCommand { get; private set; }

		#endregion Commands

	}
}
