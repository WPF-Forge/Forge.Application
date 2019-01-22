namespace Forge.Application.Demo
{
    using System;
    using System.Windows;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var controller = new DemoAppController();
            controller.ShowApplicationWindow();
        }
    }
}
