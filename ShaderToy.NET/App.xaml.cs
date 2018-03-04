using System.Windows;
using ShaderToy.NET.Helpers;

namespace ShaderToy.NET
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //Optimus Enabler (64bit)
            //this makes the assembly use the High-Performance GPU
            try
            {
                ///Ignore any System.EntryPointNotFoundException
                ///or System.DllNotFoundException exceptions here
                OptimusSupportHelper.NvAPI_Initialize_64();
            }
            catch { }

            new MainWindow();
        }

    }
}
