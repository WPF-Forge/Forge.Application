namespace Forge.Application.Routing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using Forge.Application.Commands;
    using Forge.Application.Commands.Internal;
    using Forge.Application.Commands.Internal.Menu;
    using Forge.Application.Helpers.Internal;
    using Forge.Application.Models;
    using Forge.Application.Properties;

    using MaterialDesignThemes.Wpf;

    public abstract class Route : Model
    {
        protected const PackIconKind NoIcon = (PackIconKind)(-1);

        protected static IRefreshableCommand CommandNotImplemented([CallerMemberName] string name = null)
        {
            return new UntrackedCommand(param => Console.WriteLine($"Called not implemented command {name}"));
        }

        protected internal const double SmallDialog = 250d;
        protected internal const double LargeDialog = 350d;
        protected internal const double ExtraLargeDialog = 450d;

        private const bool IgnoreNullByDefault = true;

        internal int CommandCounter = 0;
        private bool initialized;
        private bool initializing;

        private RouteConfig routeConfig;
        private IRouteStack routes;

        protected Route()
            : this(null, null)
        {
        }

        protected Route(IRouteStack routes)
            : this(routes, null)
        {
        }

        protected Route(IRouteStack routes, IRouteFactory routeFactory)
        {
            this.RouteConfig = new RouteConfig();
            this.Routes = routes;
            this.RouteFactory = routeFactory;

            this.PushMenuRouteCommand = this.AsyncCommand<Type>(this.PushMenuRoute);
            this.GoToMenuRouteCommand = this.AsyncCommand<Type>(this.GoToMenuRoute);
            this.PopRouteCommand = this.AsyncCommand<object>(this.PopRoute, false);
        }

        /// <summary>
        /// Gets whether this route is on top of the stack.
        /// </summary>
        public bool IsActive => this.Routes != null && this.Routes.Current == this;

        internal bool IsTransitionReady => this.CommandCounter <= 1;

        internal bool IsOutsideTransitionReady => this.CommandCounter == 0;

        public bool IsTransitioning { get; internal set; }

        public bool EnableCommands { get; protected set; } = true;

        public ICommand PushMenuRouteCommand { get; }

        public ICommand GoToMenuRouteCommand { get; }

        public ICommand PopRouteCommand { get; }

        /// <summary>
        /// Gets the configuration for this route.
        /// </summary>
        public RouteConfig RouteConfig
        {
            get { return this.routeConfig; }
            protected set
            {
                if (Equals(value, this.routeConfig)) return;
                this.routeConfig = value;
                this.NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the route stack this object is associated with.
        /// </summary>
        public IRouteStack Routes
        {
            get { return this.routes; }
            internal set
            {
                if (Equals(this.routes, value)) return;
                this.routes = value;
                this.NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Gives a chance to cancel route deactivation or provide confirmation.
        /// A false value indicates cancellation.
        /// </summary>
        public Func<Task<bool>> DeactivateRequested { get; protected set; }

        /// <summary>
        /// Gets the route provider. If no factory exists
        /// routes will be resolved from the route stack.
        /// </summary>
        public IRouteFactory RouteFactory { get; }

        /// <summary>
        /// Resolves a concrete route object.
        /// </summary>
        /// <typeparam name="TRoute">Concrete route type.</typeparam>
        /// <returns>A wrapper for the requested route.</returns>
        protected RouteWrapper<TRoute> GetRoute<TRoute>() where TRoute : Route => this.GetRoute<TRoute>(parameters: null);

        protected RouteWrapper<TRoute> GetRoute<TRoute>(Action<IDictionary<string, object>> parametersInitializer)
            where TRoute : Route
        {
            var parameters = new Dictionary<string, object>();
            parametersInitializer?.Invoke(parameters);
            return this.GetRoute<TRoute>(parameters);
        }

        protected RouteWrapper<TRoute> GetRoute<TRoute>(string parameterName, object parameterValue)
            where TRoute : Route
            => this.GetRoute<TRoute>(new Dictionary<string, object>
            {
                [parameterName] = parameterValue
            });

        protected RouteWrapper<TRoute> GetRoute<TRoute>(string parameter1Name, object parameter1Value,
            string parameter2Name,
            object parameter2Value) where TRoute : Route => this.GetRoute<TRoute>(new Dictionary<string, object>
            {
                [parameter1Name] = parameter1Value,
                [parameter2Name] = parameter2Value
            });

        protected RouteWrapper<TRoute> GetRoute<TRoute>(string parameter1Name, object parameter1Value,
            string parameter2Name,
            object parameter2Value, string parameter3Name, object parameter3Value) where TRoute : Route
            => this.GetRoute<TRoute>(new Dictionary<string, object>
            {
                [parameter1Name] = parameter1Value,
                [parameter2Name] = parameter2Value,
                [parameter3Name] = parameter3Value
            });

        protected RouteWrapper<TRoute> GetRoute<TRoute>(string parameter1Name, object parameter1Value,
            string parameter2Name,
            object parameter2Value, string parameter3Name, object parameter3Value, string parameter4Name,
            object parameter4Value) where TRoute : Route => this.GetRoute<TRoute>(new Dictionary<string, object>
            {
                [parameter1Name] = parameter1Value,
                [parameter2Name] = parameter2Value,
                [parameter3Name] = parameter3Value,
                [parameter4Name] = parameter4Value
            });

        protected RouteWrapper<TRoute> GetRoute<TRoute>(string parameter1Name, object parameter1Value,
            string parameter2Name,
            object parameter2Value, string parameter3Name, object parameter3Value, string parameter4Name,
            object parameter4Value, string parameter5Name, object parameter5Value) where TRoute : Route
            => this.GetRoute<TRoute>(new Dictionary<string, object>
            {
                [parameter1Name] = parameter1Value,
                [parameter2Name] = parameter2Value,
                [parameter3Name] = parameter3Value,
                [parameter4Name] = parameter4Value,
                [parameter5Name] = parameter5Value
            });

        protected RouteWrapper<TRoute> GetRoute<TRoute>(string parameter1Name, object parameter1Value,
            string parameter2Name,
            object parameter2Value, string parameter3Name, object parameter3Value, string parameter4Name,
            object parameter4Value, string parameter5Name, object parameter5Value, string parameter6Name,
            object parameter6Value) where TRoute : Route => this.GetRoute<TRoute>(new Dictionary<string, object>
            {
                [parameter1Name] = parameter1Value,
                [parameter2Name] = parameter2Value,
                [parameter3Name] = parameter3Value,
                [parameter4Name] = parameter4Value,
                [parameter5Name] = parameter5Value,
                [parameter6Name] = parameter6Value
            });

        /// <summary>
        /// Resolves a concrete route object.
        /// </summary>
        /// <typeparam name="TRoute">Concrete route type.</typeparam>
        /// <param name="parameters">Additional creation parameters.</param>
        /// <returns>A wrapper for the requested route.</returns>
        protected RouteWrapper<TRoute> GetRoute<TRoute>(IDictionary<string, object> parameters) where TRoute : Route
        {
            var factory = this.GetRouteFactory();
            return factory
                .Get<TRoute>(this, parameters)
                .With(this.InitializeTransientRoute)
                .CreateProxy();
        }

        protected RouteWrapper<Route> GetRoute(Type routeType) => this.GetRoute(routeType, null);

        protected RouteWrapper<Route> GetRoute(Type routeType, IDictionary<string, object> parameters)
        {
            var factory = this.GetRouteFactory();
            return factory
                .Get(this, routeType, parameters)
                .With(this.InitializeTransientRoute)
                .CreateProxy();
        }

        private void InitializeTransientRoute(Route route)
        {
            var transientRoute = route as TransientRoute;
            if (transientRoute != null)
            {
                transientRoute.Caller = this;
            }
        }

        private IRouteFactory GetRouteFactory()
        {
            IRouteFactory factory;
            if (this.RouteFactory != null)
            {
                factory = this.RouteFactory;
            }
            else if (this.Routes != null)
            {
                factory = this.Routes.RouteFactory;
            }
            else
            {
                throw new InvalidOperationException(ErrorMessages.MustHaveRoutes);
            }
            return factory;
        }

        protected Task PushMenuRoute(Type routeType)
        {
            this.AssertIsActiveRoute();
            return this.Routes.Push(this.Routes.FindMenuRoute(route => route.GetType() == routeType), RouteWrapperExtensions.CachedByDefault);
        }

        protected Task PushMenuRoute<TRoute>() where TRoute : Route
        {
            this.AssertIsActiveRoute();
            return this.Routes.Push(this.Routes.FindMenuRoute<TRoute>(), RouteWrapperExtensions.CachedByDefault);
        }

        protected Task GoToMenuRoute(Type routeType)
        {
            this.AssertIsActiveRoute();
            return this.Routes.Change(this.Routes.FindMenuRoute(route => route.GetType() == routeType));
        }

        protected Task GoToMenuRoute<TRoute>() where TRoute : Route
        {
            this.AssertIsActiveRoute();
            return this.Routes.Change(this.Routes.FindMenuRoute<TRoute>());
        }

        private void AssertIsActiveRoute()
        {
            if (this.Routes == null)
            {
                throw new InvalidOperationException(ErrorMessages.MustHaveRoutes);
            }

            if (this.Routes.Current != this)
            {
                throw new InvalidOperationException(ErrorMessages.MustBeActiveRoute);
            }
        }

        internal async Task OnRouteInitializing()
        {
            if (this.initializing)
            {
                return;
            }

            this.initializing = true;
            this.RouteInitializing();
            await this.RouteInitializingAsync();
        }

        protected virtual void RouteInitializing()
        {
        }

        protected virtual Task RouteInitializingAsync() => Task.CompletedTask;

        internal async Task OnRouteInitialized()
        {
            if (this.initialized)
            {
                return;
            }

            this.initialized = true;
            this.RouteInitialized();
            await this.RouteInitializedAsync();
        }

        protected virtual void RouteInitialized()
        {
        }

        protected virtual Task RouteInitializedAsync() => Task.CompletedTask;

        internal Task OnRouteActivating()
        {
            this.RouteActivating();
            return this.RouteActivatingAsync();
        }

        protected virtual void RouteActivating()
        {
        }

        protected virtual Task RouteActivatingAsync() => Task.CompletedTask;

        internal Task OnRouteActivated()
        {
            this.RouteActivated();
            return this.RouteActivatedAsync();
        }

        protected virtual void RouteActivated()
        {
        }

        protected virtual Task RouteActivatedAsync() => Task.CompletedTask;

        internal Task OnRouteHiding()
        {
            this.RouteHiding();
            return this.RouteHidingAsync();
        }

        protected virtual void RouteHiding()
        {
        }

        protected virtual Task RouteHidingAsync() => Task.CompletedTask;

        internal Task OnRouteHidden()
        {
            this.RouteHidden();
            return this.RouteHiddenAsync();
        }

        protected virtual void RouteHidden()
        {
        }

        protected virtual Task RouteHiddenAsync() => Task.CompletedTask;

        internal Task OnRouteRestoring(object result)
        {
            this.RouteRestoring(result);
            return this.RouteRestoringAsync(result);
        }

        protected virtual void RouteRestoring(object result)
        {
        }

        protected virtual Task RouteRestoringAsync(object result) => Task.CompletedTask;

        internal Task OnRouteRestored(object result)
        {
            this.RouteRestored(result);
            return this.RouteRestoredAsync(result);
        }

        protected virtual void RouteRestored(object result)
        {
        }

        protected virtual Task RouteRestoredAsync(object result) => Task.CompletedTask;

        internal Task OnRouteDeactivating(bool isChanging)
        {
            this.RouteDeactivating(isChanging);
            return this.RouteDeactivatingAsync(isChanging);
        }

        protected virtual void RouteDeactivating(bool isChanging)
        {
        }

        protected virtual Task RouteDeactivatingAsync(bool isChanging) => Task.CompletedTask;

        internal Task OnRouteDeactivated(bool isChanging)
        {
            this.RouteDeactivated(isChanging);
            return this.RouteDeactivatingAsync(isChanging);
        }

        protected virtual void RouteDeactivated(bool isChanging)
        {
        }

        protected virtual Task RouteDeactivatedAsync(bool isChanging) => Task.CompletedTask;

        internal Task OnRouteReady(RouteActivationMethod routeActivationMethod,
            [NotNull] IEnumerable<RouteEventError> errors)
        {
            var routeEventErrors = errors as IList<RouteEventError> ?? errors.ToList();
            this.RouteReady(routeActivationMethod, routeEventErrors);
            return this.RouteReadyAsync(routeActivationMethod, routeEventErrors);
        }

        protected virtual void RouteReady(RouteActivationMethod routeActivationMethod,
            [NotNull] IEnumerable<RouteEventError> errors)
        {
        }

        protected virtual Task RouteReadyAsync(RouteActivationMethod routeActivationMethod,
            [NotNull] IEnumerable<RouteEventError> errors) => Task.CompletedTask;

        /// <summary>
        /// If not null, the return value of this method will be displayed.
        /// View caching is not possible if this method is not implemented.
        /// </summary>
        protected internal virtual object CreateView(bool isReload) => null;

        protected internal virtual void ApplicationShuttingDown()
        {
        }

        protected Task PopRoute(object result)
        {
            this.AssertIsActiveRoute();
            return this.Routes.Pop(result);
        }

        protected void ReloadView()
        {
            if (this.Routes == null)
            {
                throw new InvalidOperationException(ErrorMessages.MustHaveRoutes);
            }

            this.Routes.ReloadView(this);
        }

        private static TCommand Subscribe<TCommand>(RefreshSource[] refreshSources, TCommand command)
            where TCommand : IRefreshableCommand
        {
            if (refreshSources != null)
            {
                foreach (var refreshSource in refreshSources)
                {
                    refreshSource.Commands.Add(command);
                }
            }

            return command;
        }

        internal void BeginTransition()
        {
            if (!this.IsTransitionReady)
            {
                throw new InvalidOperationException("Cannot transition routes while any command is executing.");
            }

            this.IsTransitioning = true;
        }

        // Sync

        #region Action

        protected IRefreshableCommand Command(Action execute, params RefreshSource[] refreshSources)
        {
            return Subscribe(refreshSources, new RouteActionCommand(this, execute, null));
        }

        protected IRefreshableCommand Command(Action execute, Func<bool> canExecute,
            params RefreshSource[] refreshSources)
        {
            return Subscribe(refreshSources, new RouteActionCommand(this, execute, canExecute));
        }

        #endregion

        #region Action<TClass>

        protected IRefreshableCommand Command<TParameter>(Action<TParameter> execute,
            params RefreshSource[] refreshSources) where TParameter : class
        {
            return Subscribe(refreshSources, new RouteCommand<TParameter>(this, execute, null, IgnoreNullByDefault));
        }

        protected IRefreshableCommand Command<TParameter>(Action<TParameter> execute, bool ignoreNullParameters,
            params RefreshSource[] refreshSources) where TParameter : class
        {
            return Subscribe(refreshSources, new RouteCommand<TParameter>(this, execute, null, ignoreNullParameters));
        }

        protected IRefreshableCommand Command<TParameter>(Action<TParameter> execute, Predicate<TParameter> canExecute,
            params RefreshSource[] refreshSources) where TParameter : class
        {
            return Subscribe(refreshSources,
                new RouteCommand<TParameter>(this, execute, canExecute, IgnoreNullByDefault));
        }

        protected IRefreshableCommand Command<TParameter>(Action<TParameter> execute, Predicate<TParameter> canExecute,
            bool ignoreNullParameters, params RefreshSource[] refreshSources) where TParameter : class
        {
            return Subscribe(refreshSources,
                new RouteCommand<TParameter>(this, execute, canExecute, ignoreNullParameters));
        }

        #endregion

        #region Action<TValue>

        protected IRefreshableCommand ValueCommand<TParameter>(Action<TParameter> execute,
            params RefreshSource[] refreshSources) where TParameter : struct
        {
            return Subscribe(refreshSources, new RouteValueCommand<TParameter>(this, execute, null));
        }

        protected IRefreshableCommand ValueCommand<TParameter>(Action<TParameter> execute, Predicate<object> canExecute,
            params RefreshSource[] refreshSources) where TParameter : struct
        {
            return Subscribe(refreshSources, new RouteValueCommand<TParameter>(this, execute, canExecute));
        }

        #endregion

        // Async

        #region Func<Task>

        protected IRefreshableCommand AsyncCommand(Func<Task> execute, params RefreshSource[] refreshSources)
        {
            return Subscribe(refreshSources, new AsyncRouteActionCommand(this, execute, null));
        }

        protected IRefreshableCommand AsyncCommand(Func<Task> execute, Func<bool> canExecute,
            params RefreshSource[] refreshSources)
        {
            return Subscribe(refreshSources, new AsyncRouteActionCommand(this, execute, canExecute));
        }

        #endregion

        #region Func<TClass, Task>

        protected IRefreshableCommand AsyncCommand<TParameter>(Func<TParameter, Task> execute,
            params RefreshSource[] refreshSources) where TParameter : class
        {
            return Subscribe(refreshSources, new AsyncRouteCommand<TParameter>(this, execute, null, IgnoreNullByDefault));
        }

        protected IRefreshableCommand AsyncCommand<TParameter>(Func<TParameter, Task> execute, bool ignoreNullParameters,
            params RefreshSource[] refreshSources) where TParameter : class
        {
            return Subscribe(refreshSources,
                new AsyncRouteCommand<TParameter>(this, execute, null, ignoreNullParameters));
        }

        protected IRefreshableCommand AsyncCommand<TParameter>(Func<TParameter, Task> execute,
            Predicate<TParameter> canExecute, params RefreshSource[] refreshSources) where TParameter : class
        {
            return Subscribe(refreshSources,
                new AsyncRouteCommand<TParameter>(this, execute, canExecute, IgnoreNullByDefault));
        }

        protected IRefreshableCommand AsyncCommand<TParameter>(Func<TParameter, Task> execute,
            Predicate<TParameter> canExecute, bool ignoreNullParameters, params RefreshSource[] refreshSources)
            where TParameter : class
        {
            return Subscribe(refreshSources,
                new AsyncRouteCommand<TParameter>(this, execute, canExecute, ignoreNullParameters));
        }

        #endregion

        #region Func<TValue, Task>

        protected IRefreshableCommand AsyncValueCommand<TParameter>(Func<TParameter, Task> execute,
            params RefreshSource[] refreshSources) where TParameter : struct
        {
            return Subscribe(refreshSources, new AsyncRouteValueCommand<TParameter>(this, execute, null));
        }

        protected IRefreshableCommand AsyncValueCommand<TParameter>(Func<TParameter, Task> execute,
            Predicate<object> canExecute, params RefreshSource[] refreshSources) where TParameter : struct
        {
            return Subscribe(refreshSources, new AsyncRouteValueCommand<TParameter>(this, execute, canExecute));
        }

        #endregion

        // Menu Sync

        #region Menu Action

        protected IMenuCommand Command(string commandText, PackIconKind? iconKind, Action execute,
            params RefreshSource[] refreshSources)
        {
            return Subscribe(refreshSources, new RouteActionMenuCommand(this, commandText, iconKind, execute, null));
        }

        protected IMenuCommand Command(string commandText, PackIconKind? iconKind, Action execute, Func<bool> canExecute,
            params RefreshSource[] refreshSources)
        {
            return Subscribe(refreshSources,
                new RouteActionMenuCommand(this, commandText, iconKind, execute, canExecute));
        }

        #endregion

        #region Menu Action<TClass>

        protected IMenuCommand Command<TParameter>(string commandText, PackIconKind? iconKind,
            Action<TParameter> execute, params RefreshSource[] refreshSources) where TParameter : class
        {
            return Subscribe(refreshSources,
                new RouteMenuCommand<TParameter>(this, commandText, iconKind, execute, null, IgnoreNullByDefault));
        }

        protected IMenuCommand Command<TParameter>(string commandText, PackIconKind? iconKind,
            Action<TParameter> execute, bool ignoreNullParameters, params RefreshSource[] refreshSources)
            where TParameter : class
        {
            return Subscribe(refreshSources,
                new RouteMenuCommand<TParameter>(this, commandText, iconKind, execute, null, ignoreNullParameters));
        }

        protected IMenuCommand Command<TParameter>(string commandText, PackIconKind? iconKind,
            Action<TParameter> execute, Predicate<TParameter> canExecute, params RefreshSource[] refreshSources)
            where TParameter : class
        {
            return Subscribe(refreshSources,
                new RouteMenuCommand<TParameter>(this, commandText, iconKind, execute, canExecute, IgnoreNullByDefault));
        }

        protected IMenuCommand Command<TParameter>(string commandText, PackIconKind? iconKind,
            Action<TParameter> execute, Predicate<TParameter> canExecute, bool ignoreNullParameters,
            params RefreshSource[] refreshSources) where TParameter : class
        {
            return Subscribe(refreshSources,
                new RouteMenuCommand<TParameter>(this, commandText, iconKind, execute, canExecute, ignoreNullParameters));
        }

        #endregion

        #region Menu Action<TValue>

        protected IMenuCommand ValueCommand<TParameter>(string commandText, PackIconKind? iconKind,
            Action<TParameter> execute, params RefreshSource[] refreshSources) where TParameter : struct
        {
            return Subscribe(refreshSources,
                new RouteValueMenuCommand<TParameter>(this, commandText, iconKind, execute, null));
        }

        protected IMenuCommand ValueCommand<TParameter>(string commandText, PackIconKind? iconKind,
            Action<TParameter> execute, Predicate<object> canExecute, params RefreshSource[] refreshSources)
            where TParameter : struct
        {
            return Subscribe(refreshSources,
                new RouteValueMenuCommand<TParameter>(this, commandText, iconKind, execute, canExecute));
        }

        #endregion

        // Menu Async

        #region Menu Func<Task>

        protected IMenuCommand AsyncCommand(string commandText, PackIconKind? iconKind, Func<Task> execute,
            params RefreshSource[] refreshSources)
        {
            return Subscribe(refreshSources, new AsyncRouteActionMenuCommand(this, commandText, iconKind, execute, null));
        }

        protected IMenuCommand AsyncCommand(string commandText, PackIconKind? iconKind, Func<Task> execute,
            Func<bool> canExecute, params RefreshSource[] refreshSources)
        {
            return Subscribe(refreshSources,
                new AsyncRouteActionMenuCommand(this, commandText, iconKind, execute, canExecute));
        }

        #endregion

        #region Menu Func<TClass, Task>

        protected IMenuCommand AsyncCommand<TParameter>(string commandText, PackIconKind? iconKind,
            Func<TParameter, Task> execute, params RefreshSource[] refreshSources) where TParameter : class
        {
            return Subscribe(refreshSources,
                new AsyncRouteMenuCommand<TParameter>(this, commandText, iconKind, execute, null, IgnoreNullByDefault));
        }

        protected IMenuCommand AsyncCommand<TParameter>(string commandText, PackIconKind? iconKind,
            Func<TParameter, Task> execute, bool ignoreNullParameters, params RefreshSource[] refreshSources)
            where TParameter : class
        {
            return Subscribe(refreshSources,
                new AsyncRouteMenuCommand<TParameter>(this, commandText, iconKind, execute, null, ignoreNullParameters));
        }

        protected IMenuCommand AsyncCommand<TParameter>(string commandText, PackIconKind? iconKind,
            Func<TParameter, Task> execute, Predicate<TParameter> canExecute, params RefreshSource[] refreshSources)
            where TParameter : class
        {
            return Subscribe(refreshSources,
                new AsyncRouteMenuCommand<TParameter>(this, commandText, iconKind, execute, canExecute,
                    IgnoreNullByDefault));
        }

        protected IMenuCommand AsyncCommand<TParameter>(string commandText, PackIconKind? iconKind,
            Func<TParameter, Task> execute, Predicate<TParameter> canExecute, bool ignoreNullParameters,
            params RefreshSource[] refreshSources) where TParameter : class
        {
            return Subscribe(refreshSources,
                new AsyncRouteMenuCommand<TParameter>(this, commandText, iconKind, execute, canExecute,
                    ignoreNullParameters));
        }

        #endregion

        #region Menu Func<TValue, Task>

        protected IMenuCommand AsyncValueCommand<TParameter>(string commandText, PackIconKind? iconKind,
            Func<TParameter, Task> execute, params RefreshSource[] refreshSources) where TParameter : struct
        {
            return Subscribe(refreshSources,
                new AsyncRouteValueMenuCommand<TParameter>(this, commandText, iconKind, execute, null));
        }

        protected IMenuCommand AsyncValueCommand<TParameter>(string commandText, PackIconKind? iconKind,
            Func<TParameter, Task> execute, Predicate<object> canExecute, params RefreshSource[] refreshSources)
            where TParameter : struct
        {
            return Subscribe(refreshSources,
                new AsyncRouteValueMenuCommand<TParameter>(this, commandText, iconKind, execute, canExecute));
        }

        #endregion
    }
}
