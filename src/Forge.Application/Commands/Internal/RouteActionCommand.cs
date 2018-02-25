namespace Forge.Application.Commands.Internal
{
    using System;
    using System.Threading;

    using Forge.Application.Routing;

    internal class RouteActionCommand : IRefreshableCommand
    {
        private readonly Func<bool> canExecute;
        private readonly Action execute;

        public RouteActionCommand(Route route, Action execute, Func<bool> canExecute)
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

        public bool CanExecute(object parameter)
        {
            return this.canExecute == null || this.canExecute();
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
                this.execute();
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

        public void RaiseCanExecuteChanged()
        {
            this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
