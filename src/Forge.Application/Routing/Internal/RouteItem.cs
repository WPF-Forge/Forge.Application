namespace Forge.Application.Routing.Internal
{
    using System.Threading.Tasks;

    internal class RouteItem
    {
        public RouteItem(Route route)
        {
            this.Route = route;
            this.CompletionSource = new TaskCompletionSource<object>();
        }

        public Route Route { get; }

        public TaskCompletionSource<object> CompletionSource { get; }

        public object CachedView { get; set; }
    }
}
