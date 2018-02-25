namespace Forge.Application.Commands.Internal
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Forge.Application.Routing;

    internal class AsyncRouteActionCommand : IRefreshableCommand
    {
        private readonly Func<bool> canExecute;
        private readonly Func<Task> execute;

        public AsyncRouteActionCommand(Route route, Func<Task> execute,
            Func<bool> canExecute)
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

        public async void Execute(object parameter)
        {
            if (this.Route.IsTransitioning || !this.Route.EnableCommands)
            {
                return;
            }

            try
            {
                Interlocked.Increment(ref this.Route.CommandCounter);
                await this.execute();
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
