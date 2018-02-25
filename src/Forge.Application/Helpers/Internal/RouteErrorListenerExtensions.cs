namespace Forge.Application.Helpers.Internal
{
    using System;

    using Forge.Application.Routing;

    internal static class RouteErrorListenerExtensions
    {
        public static void TryOnRouteEventException(this IRouteErrorListener listener, Route route,
            RouteEventType eventType, Exception exception)
        {
            if (listener == null)
            {
                return;
            }

            try
            {
                listener.OnRouteEventException(route, eventType, exception);
            }
            catch
            {
                // ignored
            }
        }
    }
}
