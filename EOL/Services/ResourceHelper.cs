using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows;

namespace EOL.Services
{
    public class ResourceHelper
    {
        public static Bitmap GetBitmapFromExternalResource(string relativePath)
        {
            // Get the directory where the executable is running
            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Combine the executable directory with the relative resource path
            string resourceFullPath = Path.Combine(exeDirectory, relativePath);

            // Check if the file exists
            if (!File.Exists(resourceFullPath))
            {
                throw new FileNotFoundException("Resource file not found.", resourceFullPath);
            }

            // Load the image from the file system
            using (FileStream fs = new FileStream(resourceFullPath, FileMode.Open, FileAccess.Read))
            {
                return new Bitmap(fs);
            }
        }
    }
}
