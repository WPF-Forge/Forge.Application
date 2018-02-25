namespace Forge.Application.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;

    using Forge.Application.Commands;
    using Forge.Application.Controls;
    using Forge.Application.Infrastructure.Internal;
    using Forge.Application.Properties;
    using Forge.Application.Routing;

    using MaterialDesignThemes.Wpf;

    using Ninject;
    using Ninject.Modules;

    public abstract class AppController : INotifyPropertyChanged, IRouteErrorListener,
        IMainWindowLocator, IMainWindowController
    {
        public bool CloseOnClickAway
        {
            get => this._closeOnClickAway;
            set
            {
                this._closeOnClickAway = value;

                try
                {
                    ((MaterialRoutesWindow) this.Window).RootDialog.CloseOnClickAway = value;
                }
                catch
                {
                    //Supress (Window might not be initialized yet)
                }
            }
        }

        private static int dialogId;

        private readonly int id;

        private double fontSize = 13d;
        private bool initialized;
        private bool isMenuOpen;
        private bool lockToggle;
        private ICommand menuCommand;
        private string title;
        private bool toggleState;
        private bool _closeOnClickAway;

        protected AppController()
        {
            this.id = Interlocked.Increment(ref dialogId);
            this.MenuCommand = new UntrackedCommand(async parameter =>
            {
                if (this.Routes.Count <= 1)
                {
                    this.IsMenuOpen = !this.IsMenuOpen;
                }
                else
                {
                    var current = this.Routes.Current;
                    if (current.IsTransitioning || !current.IsOutsideTransitionReady)
                    {
                        return;
                    }

                    var handler = current.DeactivateRequested;
                    var popRoute = handler == null || await handler();
                    if (popRoute)
                    {
                        await this.Routes.Pop(null);
                    }
                }
            });

            this.SnackbarMessageQueue = new SnackbarMessageQueue();
            this.Kernel = new StandardKernel();
        }

        protected internal IKernel Kernel { get; }

        protected Route InitialRoute { get; set; }

        public IRouteStack Routes { get; private set; }

        public ISnackbarMessageQueue SnackbarMessageQueue { get; }

        public bool LockToggle
        {
            get { return this.lockToggle; }
            private set
            {
                if (value == this.lockToggle) return;
                this.lockToggle = value;
                this.OnPropertyChanged();
            }
        }

        public bool ToggleState
        {
            get { return this.toggleState; }
            private set
            {
                if (value == this.toggleState) return;
                this.toggleState = value;
                this.OnPropertyChanged();
            }
        }

        public ICommand MenuCommand
        {
            get { return this.menuCommand; }
            private set
            {
                if (Equals(value, this.menuCommand)) return;
                this.menuCommand = value;
                this.OnPropertyChanged();
            }
        }

        public Window Window { get; private set; }

        protected internal string HostIdentifer => "RouteController" + this.id;

        public bool IsMenuOpen
        {
            get { return this.isMenuOpen; }
            set
            {
                if (value == this.isMenuOpen) return;
                this.isMenuOpen = value;
                this.ToggleState = value;
                this.OnPropertyChanged();
            }
        }

        public string Title
        {
            get { return this.title; }
            set
            {
                if (value == this.title) return;
                this.title = value;
                this.OnPropertyChanged();
            }
        }

        public double FontSize
        {
            get { return this.fontSize; }
            set
            {
                if (value.Equals(this.fontSize)) return;
                this.fontSize = value;
                this.OnPropertyChanged();
            }
        }

        public Window GetMainWindow() => this.Window;

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnRouteEventException(Route route, RouteEventType eventType, Exception exception)
        {
        }

        public virtual void OnRouteCommandException(Route route, ICommand command, Exception exception)
        {
        }

        public void Exit()
        {
            this.Window?.Close();
        }

        public void ShowApplicationWindow()
        {
            if (this.Window != null)
            {
                throw new InvalidOperationException(
                    "Application window has already been shown for current controller.");
            }

            this.Initialize();
            if (this.InitialRoute == null)
            {
                throw new InvalidOperationException("No initial route has been specified.");
            }

            this.Window = this.CreateMainWindow();

            this.Window.Closing += this.OnWindowClosing;
            this.Window.Show();
            var initialRoute = this.InitialRoute;
            this.InitialRoute = null;
            this.Routes.Change(initialRoute);
        }

        private async void OnWindowClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            var close = await this.CloseRequested();
            if (close)
            {
                try
                {
                    this.Routes.Current.ApplicationShuttingDown();
                }
                catch
                {
                    // ignored
                }

                this.Kernel.Dispose();
                System.Windows.Application.Current.Shutdown();
            }
        }

        protected virtual Task<bool> CloseRequested()
        {
            return Task.FromResult(true);
        }

        protected virtual void OnInitializing()
        {
        }

        protected virtual void OnInitialized()
        {
        }

        protected virtual Window CreateMainWindow()
        {
            return new MaterialRoutesWindow(this)
            {
                RootDialog = {Identifier = this.HostIdentifer, CloseOnClickAway = this.CloseOnClickAway}
            };
        }

        protected virtual Route GetInitialRoute() => this.InitialRoute ?? this.Routes.MenuRoutes.First();

        protected virtual IEnumerable<INinjectModule> LoadModules()
        {
            return null;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Initialize()
        {
            if (this.initialized)
            {
                return;
            }

            this.Kernel.Load(new DefaultAppModule(this));
            var modules = this.LoadModules();
            if (modules != null)
            {
                this.Kernel.Load(modules);
            }

            this.Routes = this.Kernel.Get<IRouteStack>();
            this.Routes.RouteHeadChanged += (sender, args) =>
            {
                this.IsMenuOpen = false;
                this.ToggleState = this.LockToggle = this.Routes.Count > 1;
            };

            this.Kernel.Inject(this);
            this.OnInitializing();
            foreach (var route in this.Routes.MenuRoutes)
            {
                if (route == null)
                {
                    continue;
                }

                route.Routes = this.Routes;
            }

            this.InitialRoute = this.GetInitialRoute();
            this.InitialRoute.Routes = this.Routes;
            this.OnInitialized();
            this.initialized = true;
        }
    }
}