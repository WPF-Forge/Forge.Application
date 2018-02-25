namespace Forge.Application.Infrastructure.Internal
{
    using System;
    using System.Windows.Threading;

    internal class DispatcherContext : IContext
    {
        private readonly Dispatcher dispatcher;

        public DispatcherContext(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        public bool IsSynchronized => this.dispatcher.CheckAccess();

        public void Invoke(Action action) => this.dispatcher.Invoke(action);
    }
}
