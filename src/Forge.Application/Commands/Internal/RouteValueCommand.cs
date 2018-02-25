namespace Forge.Application.Commands.Internal
{
    using System;
    using System.Threading;

    using Forge.Application.Routing;

    internal class RouteValueCommand<TParameter> : IRefreshableCommand where TParameter : struct
    {
        private readonly Predicate<object> canExecute;
        private readonly Action<TParameter> execute;

        public RouteValueCommand(Route route, Action<TParameter> execute,
            Predicate<object> canExecute)
        {
            if (route == null)
            {
                throw new ArgumentNullException(nameof(route));
            }

            this.Route = route;
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public Route Route { get; }

        public event EventHandler CanExecuteChanged;

        public void RaiseCanExecuteChanged()
        {
            this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool CanExecute(object parameter)
        {
            return this.canExecute == null || this.canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            if (this.Route.IsTransitioning || !this.Route.EnableCommands)
            {
                return;
            }

            try
            {
                Interlocked.Increment(ref this.Route.CommandCounter);
                if (!(parameter is TParameter))
                {
                    return;
                }

                this.execute((TParameter)parameter);
            }
            catch (Exception e)
            {
                if (this.Route.Routes?.RouteErrorListener != null)
                {
                    this.Route.Routes.RouteErrorListener.OnRouteCommandException(this.Route, this, e);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                Interlocked.Decrement(ref this.Route.CommandCounter);
            }
        }
    }
}
