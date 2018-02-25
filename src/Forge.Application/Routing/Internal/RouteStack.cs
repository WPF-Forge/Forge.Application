namespace Forge.Application.Routing.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows;

    using Forge.Application.Helpers.Internal;
    using Forge.Application.Infrastructure;
    using Forge.Application.Properties;

    internal class RouteStack : IRouteStack
    {
        private readonly Stack<RouteItem> stack;
        private readonly object syncRoot = new object();
        private bool locked;

        private RouteItem routeHead;
        private object menuHeader;

        public RouteStack(ObservableCollection<Route> menuRoutes, IRouteFactory routeFactory,
            IRouteErrorListener routeErrorListener, IContext synchronizationContext)
        {
            if (routeFactory == null || routeErrorListener == null || synchronizationContext == null)
            {
                throw new ArgumentNullException();
            }

            this.MenuRoutes = menuRoutes ?? new ObservableCollection<Route>();
            this.RouteFactory = routeFactory;
            this.RouteErrorListener = routeErrorListener;
            this.SynchronizationContext = synchronizationContext;
            this.stack = new Stack<RouteItem>();
        }

        private RouteItem RouteHead
        {
            set
            {
                if (this.routeHead == value) return;
                var previous = this.routeHead;
                this.routeHead = value;
                if (value.Route != null)
                {
                    value.Route.IsTransitioning = false;
                }

                this.OnPropertyChanged(nameof(this.Current));
                this.OnPropertyChanged(nameof(this.CurrentView));
                this.OnHeadChanged();
                previous?.Route.NotifyPropertyChanged(nameof(Route.IsActive));
                this.routeHead?.Route.NotifyPropertyChanged(nameof(Route.IsActive));
            }
        }

        private LockManager Lock => new LockManager(this, true);

        private LockManager SoftLock => new LockManager(this, false);

        public event EventHandler RouteHeadChanged;

        public IRouteFactory RouteFactory { get; }

        public object MenuHeader
        {
            get { return this.menuHeader; }
            set
            {
                if (Equals(value, this.menuHeader)) return;
                this.menuHeader = value;
                this.OnPropertyChanged();
            }
        }

        public ObservableCollection<Route> MenuRoutes { get; }

        public IRouteErrorListener RouteErrorListener { get; }

        public IContext SynchronizationContext { get; }

        public int Count => this.stack.Count;

        public Route Current => this.routeHead?.Route;

        public object CurrentView
        {
            get
            {
                if (this.routeHead == null)
                {
                    return null;
                }

                return this.routeHead.CachedView ?? this.routeHead.Route;
            }
        }

        public async Task<object> Push(Route route, bool cacheCurrentView)
        {
            this.SynchronizationContext.VerifyAccess();

            if (route == null)
            {
                throw new ArgumentNullException(nameof(route));
            }

            if (route.Routes != null && route.Routes != this)
            {
                throw new ArgumentException(ErrorMessages.RoutesAssociatedWithOtherStack);
            }

            if (route is TransientRoute)
            {
                throw new ArgumentException("Cannot push a transient route.");
            }

            if (this.stack.Any(item => item.Route == route))
            {
                throw new InvalidOperationException("Cannot push same route instance multiple times.");
            }

            List<RouteEventError> errors;
            Task<object> result;
            using (this.Lock)
            {
                errors = new List<RouteEventError>();
                result = (await this.PushRoute(route, cacheCurrentView, false, errors)).Task;
            }

            await this.OnRouteReady(route, RouteActivationMethod.Pushed, errors);
            return await result;
        }

        public async Task Change(Route route)
        {
            this.SynchronizationContext.VerifyAccess();

            if (route == null)
            {
                throw new ArgumentNullException(nameof(route));
            }

            if (route == this.Current)
            {
                return;
            }

            if (route.Routes != null && route.Routes != this)
            {
                throw new ArgumentException(ErrorMessages.RoutesAssociatedWithOtherStack);
            }

            var completionSources = new List<TaskCompletionSource<object>>(this.stack.Count);
            var errors = new List<RouteEventError>();
            using (this.Lock)
            {
                await this.ChangeRoute(route, completionSources, errors);
            }

            await this.OnRouteReady(route, RouteActivationMethod.Changed, errors);
            foreach (var completionSource in completionSources)
            {
                completionSource.SetCanceled();
            }
        }

        public async Task<Route> Pop(object result)
        {
            this.SynchronizationContext.VerifyAccess();

            if (this.stack.Count <= 1)
            {
                throw new InvalidOperationException("Cannot pop base route.");
            }

            RouteItem poppedRoute;
            RouteItem nextRoute;
            List<RouteEventError> errors;
            using (this.Lock)
            {
                poppedRoute = this.stack.Pop();
                nextRoute = this.stack.Peek();
                errors = new List<RouteEventError>();

                try
                {
                    await poppedRoute.Route.OnRouteDeactivating(false);
                }
                catch (Exception ex)
                {
                    this.RouteErrorListener.TryOnRouteEventException(poppedRoute.Route, RouteEventType.Deactivating, ex);
                }

                try
                {
                    await nextRoute.Route.OnRouteRestoring(result);
                }
                catch (Exception ex)
                {
                    errors.Add(new RouteEventError(RouteEventType.Restoring, ex));
                    this.RouteErrorListener.TryOnRouteEventException(nextRoute.Route, RouteEventType.Restoring, ex);
                }

                poppedRoute.CachedView = null;
                var route = nextRoute.Route;
                if (nextRoute.CachedView == null)
                {
                    try
                    {
                        var view = route.CreateView(false);
                        nextRoute.CachedView = view;
                        var frameworkElement = view as FrameworkElement;
                        if (frameworkElement != null && frameworkElement.DataContext == null)
                        {
                            frameworkElement.DataContext = route;
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add(new RouteEventError(RouteEventType.ViewCreation, ex));
                        this.RouteErrorListener.TryOnRouteEventException(route, RouteEventType.ViewCreation, ex);
                    }
                }

                this.RouteHead = nextRoute;

                try
                {
                    await poppedRoute.Route.OnRouteDeactivated(false);
                }
                catch (Exception ex)
                {
                    this.RouteErrorListener.TryOnRouteEventException(poppedRoute.Route, RouteEventType.Deactivated, ex);
                }

                try
                {
                    await nextRoute.Route.OnRouteRestored(result);
                }
                catch (Exception ex)
                {
                    errors.Add(new RouteEventError(RouteEventType.Restored, ex));
                    this.RouteErrorListener.TryOnRouteEventException(nextRoute.Route, RouteEventType.Restored, ex);
                }
            }

            await this.OnRouteReady(nextRoute.Route, RouteActivationMethod.Restored, errors);
            poppedRoute.CompletionSource.SetResult(result);
            return nextRoute.Route;
        }

        public void ReloadView(Route route)
        {
            if (route == null)
            {
                return;
            }

            using (this.SoftLock)
            {
                if (this.routeHead == null)
                {
                    return;
                }

                if (this.SynchronizationContext.IsSynchronized)
                {
                    this.ReloadViewInternal(route);
                }
                else
                {
                    this.SynchronizationContext.Invoke(() => this.ReloadViewInternal(route));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void ReloadViewInternal(Route route)
        {
            if (this.routeHead.Route == route)
            {
                try
                {
                    var view = route.CreateView(true);
                    this.routeHead.CachedView = view;
                    var frameworkElement = view as FrameworkElement;
                    if (frameworkElement != null && frameworkElement.DataContext == null)
                    {
                        frameworkElement.DataContext = route;
                    }

                    this.OnPropertyChanged(nameof(this.CurrentView));
                }
                catch (Exception ex)
                {
                    this.RouteErrorListener.TryOnRouteEventException(route, RouteEventType.ViewCreation, ex);
                }
            }
            else
            {
                var routeItem = this.stack.FirstOrDefault(item => item.Route == route);
                if (routeItem != null)
                {
                    routeItem.CachedView = null;
                }
            }
        }

        private async Task ChangeRoute(Route route, List<TaskCompletionSource<object>> completionSources,
            List<RouteEventError> errors)
        {
            var sentinel = 0;
            while (true)
            {
                if (++sentinel == 32)
                {
                    throw new RouteTransitionException("Detected possible loop with transient routes.");
                }

                var transientRoute = route as TransientRoute;
                if (transientRoute != null)
                {
                    transientRoute.Routes = this;
                    var transientRouteErrors = new List<RouteEventError>();
                    try
                    {
                        await transientRoute.OnRouteInitializing();
                    }
                    catch (Exception ex)
                    {
                        transientRouteErrors.Add(new RouteEventError(RouteEventType.Initializing, ex));
                    }

                    try
                    {
                        await transientRoute.OnRouteActivating();
                    }
                    catch (Exception ex)
                    {
                        transientRouteErrors.Add(new RouteEventError(RouteEventType.Activating, ex));
                    }

                    try
                    {
                        route = transientRoute.GetNextRoute(transientRouteErrors);
                    }
                    catch (Exception ex)
                    {
                        throw new RouteTransitionException(
                            "A transient route threw an exception while switching to next route.", ex);
                    }

                    if (route == null)
                    {
                        throw new RouteTransitionException("A transient route resulted in a dead end.");
                    }

                    if (route.Routes != null && route.Routes != this)
                    {
                        throw new RouteTransitionException(ErrorMessages.RoutesAssociatedWithOtherStack);
                    }

                    continue;
                }

                while (this.stack.Count != 0)
                {
                    var item = this.stack.Pop();
                    completionSources.Add(item.CompletionSource);
                    try
                    {
                        await item.Route.OnRouteDeactivating(true);
                    }
                    catch (Exception ex)
                    {
                        this.RouteErrorListener.TryOnRouteEventException(item.Route, RouteEventType.Deactivating, ex);
                    }

                    item.CachedView = null;

                    try
                    {
                        await item.Route.OnRouteDeactivated(true);
                    }
                    catch (Exception ex)
                    {
                        this.RouteErrorListener.TryOnRouteEventException(item.Route, RouteEventType.Deactivated, ex);
                    }
                }

                await this.PushRoute(route, false, true, errors);
                break;
            }
        }

        private async Task<TaskCompletionSource<object>> PushRoute(Route route, bool cacheCurrentView,
            bool ignoreCurrent, List<RouteEventError> errors)
        {
            route.Routes = this;

            var currentRoute = this.Current;
            if (currentRoute != null && !cacheCurrentView)
            {
                this.routeHead.CachedView = null;
            }

            if (!ignoreCurrent && currentRoute != null)
            {
                try
                {
                    await currentRoute.OnRouteHiding();
                }
                catch (Exception ex)
                {
                    this.RouteErrorListener.TryOnRouteEventException(currentRoute, RouteEventType.Hiding, ex);
                }
            }

            try
            {
                await route.OnRouteInitializing();
            }
            catch (Exception ex)
            {
                errors.Add(new RouteEventError(RouteEventType.Initializing, ex));
                this.RouteErrorListener.TryOnRouteEventException(route, RouteEventType.Initializing, ex);
            }

            try
            {
                await route.OnRouteActivating();
            }
            catch (Exception ex)
            {
                errors.Add(new RouteEventError(RouteEventType.Activating, ex));
                this.RouteErrorListener.TryOnRouteEventException(route, RouteEventType.Activating, ex);
            }

            var item = new RouteItem(route);
            try
            {
                var view = route.CreateView(false);
                item.CachedView = view;
                var frameworkElement = view as FrameworkElement;
                if (frameworkElement != null && frameworkElement.DataContext == null)
                {
                    frameworkElement.DataContext = route;
                }
            }
            catch (Exception ex)
            {
                errors.Add(new RouteEventError(RouteEventType.ViewCreation, ex));
                this.RouteErrorListener.TryOnRouteEventException(route, RouteEventType.ViewCreation, ex);
            }

            this.stack.Push(item);
            this.RouteHead = item;

            if (!ignoreCurrent && currentRoute != null)
            {
                try
                {
                    await currentRoute.OnRouteHidden();
                }
                catch (Exception ex)
                {
                    this.RouteErrorListener.TryOnRouteEventException(currentRoute, RouteEventType.Hidden, ex);
                }
            }

            try
            {
                await route.OnRouteInitialized();
            }
            catch (Exception ex)
            {
                errors.Add(new RouteEventError(RouteEventType.Initialized, ex));
                this.RouteErrorListener.TryOnRouteEventException(route, RouteEventType.Initialized, ex);
            }

            try
            {
                await route.OnRouteActivated();
            }
            catch (Exception ex)
            {
                errors.Add(new RouteEventError(RouteEventType.Activated, ex));
                this.RouteErrorListener.TryOnRouteEventException(route, RouteEventType.Activated, ex);
            }

            return item.CompletionSource;
        }

        private async Task OnRouteReady(Route route, RouteActivationMethod method, List<RouteEventError> errors)
        {
            try
            {
                await route.OnRouteReady(method, errors);
            }
            catch (Exception ex)
            {
                this.RouteErrorListener.TryOnRouteEventException(route, RouteEventType.Ready, ex);
            }
        }

        private void EnterLock(bool hardLock)
        {
            // This is old code because locking isn't necessary since we're synchronized.
            lock (this.syncRoot)
            {
                if (this.locked)
                {
                    throw new InvalidOperationException("A route change is already in progress.");
                }

                if (hardLock)
                {
                    this.Current?.BeginTransition();
                }

                this.locked = true;
            }
        }

        private void ExitLock()
        {
            lock (this.syncRoot)
            {
                this.locked = false;
            }
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnHeadChanged()
        {
            try
            {
                this.RouteHeadChanged?.Invoke(this, EventArgs.Empty);
            }
            catch
            {
                // ignored
            }
        }

        private class LockManager : IDisposable
        {
            private readonly RouteStack routeStack;

            public LockManager(RouteStack routeStack, bool hardLock)
            {
                routeStack.EnterLock(hardLock);
                this.routeStack = routeStack;
            }

            public void Dispose()
            {
                this.routeStack.ExitLock();
            }
        }
    }
}
