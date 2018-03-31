using System;
using System.Windows;
using Forge.Application.Template.Infrastructure;

namespace Forge.Application.Template
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var app = new App();
            app.InitializeComponent();
            app.Run();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var controller = new Controller();
            controller.ShowApplicationWindow();
        }
    }
}
