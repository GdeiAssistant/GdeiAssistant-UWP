using System;
using System.IO;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace GdeiAssistant.Tools
{
    public class ImageUtils
    {
        public static async Task<BitmapImage> Base64ToBitmapAsync(string source)
        {
            var byteArray = Convert.FromBase64String(source);
            BitmapImage bitmap = new BitmapImage();
            using (MemoryStream stream = new MemoryStream(byteArray))
            {
                await bitmap.SetSourceAsync(stream.AsRandomAccessStream());
            }
            return bitmap;
        }
    }
}
