namespace Forge.Application.Commands.Internal
{
    using System;
    using System.Threading;

    using Forge.Application.Routing;

    internal class RouteCommand<TParameter> : IRefreshableCommand where TParameter : class
    {
        private readonly Predicate<TParameter> canExecute;
        private readonly Action<TParameter> execute;
        private readonly bool ignoreNullParameters;

        public RouteCommand(Route route, Action<TParameter> execute,
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

        public void RaiseCanExecuteChanged()
        {
            this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool CanExecute(object parameter)
        {
            return this.canExecute == null || this.canExecute(parameter as TParameter);
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
                var param = parameter as TParameter;
                if (this.ignoreNullParameters && param == null)
                {
                    return;
                }

                this.execute(param);
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
