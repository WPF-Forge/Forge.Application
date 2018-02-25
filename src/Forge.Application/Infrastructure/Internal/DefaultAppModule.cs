namespace Forge.Application.Infrastructure.Internal
{
    using System.Collections.ObjectModel;
    using System.Diagnostics;

    using Forge.Application.Routing;
    using Forge.Application.Routing.Internal;

    using Ninject.Modules;

    internal class DefaultAppModule : NinjectModule
    {
        private readonly AppController appController;

        public DefaultAppModule(AppController appController)
        {
            this.appController = appController;
        }

        public override void Load()
        {
            // Infrastructure
            this.Bind<IServiceLocator>().ToConstant(new NinjectServiceLocator(this.appController.Kernel));
            this.Bind<AppController>().ToConstant(this.appController);
            this.Bind<IMainWindowController>().ToConstant(this.appController);
            this.Bind<IMainWindowLocator>().ToConstant(this.appController);

            var dispatcher = System.Windows.Application.Current?.Dispatcher;
            if (dispatcher != null)
            {
                this.Bind<IContext>().ToConstant(new DispatcherContext(dispatcher));
            }
            else
            {
                Debug.WriteLine("Warning: Using deferred dispatcher resolution.");
                this.Bind<IContext>().ToMethod(ctx => new DispatcherContext(System.Windows.Application.Current.Dispatcher));
            }

            // Routes
            this.Bind<IRouteErrorListener>().ToConstant(this.appController);

            this.Bind<IRouteFactory>()
                .To<ServiceLocatorRouteFactory>();

            this.Bind<IRouteStack>()
                .To<RouteStack>()
                .InSingletonScope()
                .WithConstructorArgument("menuRoutes", new ObservableCollection<Route>());

            // Services
            this.Bind<ILocalizationService>()
                .To<XamlLocalizationService>()
                .InSingletonScope();

            this.Bind<IPaletteService>()
                .To<PaletteService>()
                .InSingletonScope();

            this.Bind<INotificationService>()
                .To<SnackbarNotificationService>()
                .InSingletonScope()
                .WithConstructorArgument("snackbarMessageQueue", this.appController.SnackbarMessageQueue);

            //Bind<IDialogService>()
            //    .To<DialogHostService>();

            this.Bind<IFilePicker>()
                .To<DialogFilePicker>();
            this.Bind<IFileSaver>()
                .To<DialogFileSaver>();
        }
    }
}
