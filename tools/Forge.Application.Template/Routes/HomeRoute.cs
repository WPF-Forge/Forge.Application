using Forge.Application.Routing;
using MaterialDesignThemes.Wpf;

namespace Forge.Application.Template.Routes
{
    public class HomeRoute : Route
    {
        public HomeRoute()
        {
            this.RouteConfig.Title = "Home";
            this.RouteConfig.Icon = PackIconKind.Home;
        }
    }
}
