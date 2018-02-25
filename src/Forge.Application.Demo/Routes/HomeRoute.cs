namespace Forge.Application.Demo.Routes
{
    using Forge.Application.Routing;

    using MaterialDesignThemes.Wpf;

    public class HomeRoute : Route
    {
        public HomeRoute()
        {
            this.RouteConfig.Title = "Home";
            this.RouteConfig.Icon = PackIconKind.Home;
        }
    }
}
