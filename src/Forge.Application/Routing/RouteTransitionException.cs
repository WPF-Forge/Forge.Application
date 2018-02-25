namespace Forge.Application.Routing
{
    using System;

    public class RouteTransitionException : Exception
    {
        public RouteTransitionException(string message)
            : base(message)
        {
        }

        public RouteTransitionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
