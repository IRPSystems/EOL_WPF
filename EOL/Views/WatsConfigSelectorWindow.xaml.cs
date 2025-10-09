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
using FlashingToolLib;
using MahApps.Metro.Controls;

namespace EOL.Views
{
    /// <summary>
    /// Interaction logic for WatsConfigSelectorWindow.xaml
    /// </summary>
    public partial class WatsConfigSelectorWindow : MetroWindow
    {
        private bool _authChecked; // guard so we only prompt once

        public WatsConfigSelectorWindow(WatsConfigSelectorViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;

            // Show password dialog as soon as this window is loaded/shown
            ContentRendered += (_, __) =>
            {
                if (string.IsNullOrEmpty(vm.eolSettings.StationType) || vm.eolSettings.PackageId == Guid.Empty)
                {
                    vm.IsContinueEnabled = false;
                    var pw = new PasswordWindow { Owner = this, Title = "First Time Setup - Enter Admin Password" };
                    if (pw.ShowDialog() != true)
                    {
                        DialogResult = false; // if this window is shown modally
                        Close();
                    }
                }
            };

            vm.PasswordRequested += () =>
            {
                var pw = new PasswordWindow { Owner = this, Title = "Verify Admin" };
                if (pw.ShowDialog() != true)
                    return false;
                return true;
            };

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
        }

    }
}
