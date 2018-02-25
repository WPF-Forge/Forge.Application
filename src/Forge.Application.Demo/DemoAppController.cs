namespace Forge.Application.Demo
{
    using Forge.Application.Demo.Routes;
    using Forge.Application.Infrastructure;
    using Forge.Application.Routing;

    public class DemoAppController : AppController
    {
        protected override void OnInitializing()
        {
            var factory = this.Routes.RouteFactory;
            this.Routes.MenuRoutes.Add(this.InitialRoute = factory.Get<HomeRoute>());
            this.Routes.MenuRoutes.Add(factory.Get<AboutRoute>());
        }
    }
}
