using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace ShaderToy.NET.Helpers
{
    static class ResourceHelper
    {
        public static string LoadTextFromRecource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

	    public static BitmapImage LoadImageFromRecource(string resourceName)
	    {
			var assembly = Assembly.GetExecutingAssembly();
		    using (Stream stream = assembly.GetManifestResourceStream(resourceName)) 
			{
				var bitmap = new BitmapImage();
				bitmap.BeginInit();
				bitmap.StreamSource = stream;
				bitmap.CacheOption = BitmapCacheOption.OnLoad;
				bitmap.EndInit();
				bitmap.Freeze();
				return bitmap;
			}
		}
    }
}
