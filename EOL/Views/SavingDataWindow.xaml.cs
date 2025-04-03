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

using System.Windows;

namespace EOL.Views
{
    public partial class SavingDataWindow : Window
    {
        public SavingDataWindow()
        {
            InitializeComponent();
        }

        // Method to update the progress bar value.
        public void SetProgress(double progress)
        {
            // Update the UI element on the Dispatcher thread.
            Dispatcher.Invoke(() =>
            {
                progressBar.Value = progress;
            });
        }
    }
}

