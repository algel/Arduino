using System.Windows;
using Stylet.Xaml;

namespace Solid.Arduino.Monitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        /// <summary>Raises the <see cref="E:System.Windows.Application.Startup" /> event.</summary>
        /// <param name="e">A <see cref="T:System.Windows.StartupEventArgs" /> that contains the event data.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            //var appLoader = new ApplicationLoader
            //{
            //    Bootstrapper = new Bootstrapper(),
            //    LoadStyletResources = true
            //};
            //Resources = appLoader;
            ////Resources.MergedDictionaries.Add(appLoader);

            //Resources.MergedDictionaries.Add(new AppResources());

            base.OnStartup(e);
        }
    }
}
