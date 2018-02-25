namespace Forge.Application.Routing.Internal
{
    using System;
    using System.Threading.Tasks;

    using Forge.Application.Helpers.Internal;

    internal class RouteWrapperInternal<TRoute> : IRouteWrapper<TRoute> where TRoute : Route
    {
        public RouteWrapperInternal(Route caller, TRoute route)
        {
            if (caller == null)
            {
                throw new ArgumentNullException(nameof(caller));
            }

            if (caller.Routes == null)
            {
                throw new ArgumentException(ErrorMessages.MustHaveRoutes);
            }

            if (route == null)
            {
                throw new ArgumentNullException(nameof(route));
            }

            this.Caller = caller;
            this.Route = route;
            if (route.Routes == null)
            {
                route.Routes = caller.Routes;
            }
        }

        internal Route Caller { get; }

        public TRoute Route { get; }

        public Task<object> Push(bool cacheCurrentView)
        {
            if (this.Caller.Routes.Current != this.Caller)
            {
                throw new InvalidOperationException(ErrorMessages.MustBeActiveRoute);
            }

            return this.Caller.Routes.Push(this.Route, cacheCurrentView);
        }

        public Task Change()
        {
            if (this.Caller.Routes.Current != this.Caller)
            {
                throw new InvalidOperationException(ErrorMessages.MustBeActiveRoute);
            }

            return this.Caller.Routes.Change(this.Route);
        }
    }
}
