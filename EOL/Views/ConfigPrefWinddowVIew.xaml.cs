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
using System.Windows.Shapes;

namespace EOL.Views
{
	/// <summary>
	/// Interaction logic for ConfigPrefWinddowVIew.xaml
	/// </summary>
	public partial class ConfigPrefWinddowVIew : MetroWindow
	{
		public ConfigPrefWinddowVIew()
		{
			InitializeComponent();
		}

		private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			ConfigPrefVIewModel configPrefVIewModel = (ConfigPrefVIewModel)DataContext;
			configPrefVIewModel.CloseEvent += ConfigPrefVIewModel_CloseEvent;

		}

		private void ConfigPrefVIewModel_CloseEvent()
		{
			Close();
		}
	}
}
