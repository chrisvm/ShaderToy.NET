using System.IO;
using System.Reflection;

namespace ShaderToy.NET.Helpers
{
    static class ResourceHelper
    {
        public static string LoadTextFromRecource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            //resourceName = "MyCompany.MyProduct.MyFile.txt";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
