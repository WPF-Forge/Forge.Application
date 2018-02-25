namespace Forge.Application.Demo.Routes
{
    using System.Threading.Tasks;
    using System.Windows.Input;

    using Forge.Application.Infrastructure;
    using Forge.Application.Routing;

    using MaterialDesignThemes.Wpf;

    public class AboutRoute : Route
    {
        private readonly INotificationService notificationService;

        public AboutRoute(INotificationService notificationService)
        {
            this.notificationService = notificationService;
            this.RouteConfig.Title = "About";
            this.RouteConfig.Icon = PackIconKind.Information;

            // Initialize commands.
            this.HomeCommand = this.Command(this.Home);
            this.ContactCommand = this.AsyncCommand(this.Contact);
        }

        public ICommand HomeCommand { get; }

        public ICommand ContactCommand { get; }

        private void Home()
        {
            this.GoToMenuRoute<HomeRoute>();
        }

        private async Task Contact()
        {
            var result = await this.GetRoute<ContactRoute>("name", "Guest").Push();
            if (result is true)
            {
                this.notificationService.Notify("E-mail sent.");
            }
        }
    }
}
