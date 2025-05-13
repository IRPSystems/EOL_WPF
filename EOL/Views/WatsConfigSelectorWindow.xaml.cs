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
using EOL.ViewModels;
using MahApps.Metro.Controls;

namespace EOL.Views
{
    /// <summary>
    /// Interaction logic for WatsConfigSelectorWindow.xaml
    /// </summary>
    public partial class WatsConfigSelectorWindow : MetroWindow
    {
        public WatsConfigSelectorWindow(WatsConfigSelectorViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            vm.CloseEvent += () =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (IsLoaded && IsVisible)
                    {
                        DialogResult = true;
                        Close();
                    }
                });            //Loaded +=  (s, e) => vm.LoadPackages();
            };
    }    }
}
