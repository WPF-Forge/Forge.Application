namespace Forge.Application.Commands.Internal
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Forge.Application.Routing;

    internal class AsyncRouteCommand<TParameter> : IRefreshableCommand where TParameter : class
    {
        private readonly Predicate<TParameter> canExecute;
        private readonly Func<TParameter, Task> execute;
        private readonly bool ignoreNullParameters;

        public AsyncRouteCommand(Route route, Func<TParameter, Task> execute,
            Predicate<TParameter> canExecute, bool ignoreNullParameters)
        {
            if (route == null)
            {
                throw new ArgumentNullException(nameof(route));
            }

            this.Route = route;
            this.execute = execute;
            this.canExecute = canExecute;
            this.ignoreNullParameters = ignoreNullParameters;
        }

        public Route Route { get; }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return this.canExecute == null || this.canExecute(parameter as TParameter);
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
                var param = parameter as TParameter;
                if (this.ignoreNullParameters && param == null)
                {
                    return;
                }

                await this.execute(param);
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
