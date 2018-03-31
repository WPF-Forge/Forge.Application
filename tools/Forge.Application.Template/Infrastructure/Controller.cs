using Forge.Application.Infrastructure;
using Forge.Application.Routing;
using Forge.Application.Template.Routes;

namespace Forge.Application.Template.Infrastructure
{
    public class Controller : AppController
    {
        protected override void OnInitializing()
        {
            var factory = this.Routes.RouteFactory;
            this.Routes.MenuRoutes.Add(this.InitialRoute = factory.Get<HomeRoute>());
        }
    }
}
