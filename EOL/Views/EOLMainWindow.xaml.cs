﻿using EOL.Services;
using EOL.ViewModels;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EOL.Views
{
	/// <summary>
	/// Interaction logic for EOLMainWindow.xaml
	/// </summary>
	public partial class EOLMainWindow : MetroWindow
	{
		public EOLMainWindow()
		{
			InitializeComponent();

			UserConfigManager userConfigManager = new UserConfigManager();

			userConfigManager.ReadConfig();

            DataContext = new EOLMainViewModel();
		}
	}
}
