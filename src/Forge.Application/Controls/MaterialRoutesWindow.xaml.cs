namespace Forge.Application.Controls
{
    using System;
    using System.Windows;
    using System.Windows.Media;

    using Forge.Application.Infrastructure;
    using Forge.Application.Routing;

    using MahApps.Metro.Controls;

    using MaterialDesignThemes.Wpf;

    /// <summary>
    /// Interaction logic for MaterialRoutesWindow.xaml
    /// </summary>
    public partial class MaterialRoutesWindow : MetroWindow, IDialogHostContainer
    {
        private readonly AppController controller;

        public MaterialRoutesWindow(AppController controller)
        {
            this.DataContext = controller ?? throw new ArgumentNullException(nameof(controller));
            this.controller = controller;
            InitializeComponent();
        }

        public object CurrentView => VisualTreeHelper.GetChild(this.RouteContentPresenter, 0);

        public DialogHost GetRootDialog()
        {
            return this.RootDialog;
        }

        private void MenuRoute_Click(object sender, RoutedEventArgs e)
        {
            if (!this.controller.IsMenuOpen)
            {
                return;
            }

            this.controller.IsMenuOpen = false;
            var route = ((FrameworkElement)sender).DataContext as Route;
            this.controller.Routes.Change(route);
        }
    }
}
