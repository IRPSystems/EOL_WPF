using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EOL.Services
{
    public static class ResourceImageParser
    {
        public static byte[] ImageToByteArray(Bitmap resource)
        {
            using (var stream = new System.IO.MemoryStream())
            {
                resource.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }
    }
}
