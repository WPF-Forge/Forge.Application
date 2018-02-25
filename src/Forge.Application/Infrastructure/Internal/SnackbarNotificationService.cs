namespace Forge.Application.Infrastructure.Internal
{
    using System;

    using MaterialDesignThemes.Wpf;

    internal class SnackbarNotificationService : INotificationService
    {
        private readonly ISnackbarMessageQueue snackbarMessageQueue;
        private readonly IMainWindowLocator windowLocator;

        private string cacheBreaker = string.Empty;

        public SnackbarNotificationService(ISnackbarMessageQueue snackbarMessageQueue, IMainWindowLocator windowLocator)
        {
            this.snackbarMessageQueue = snackbarMessageQueue;
            this.windowLocator = windowLocator;
        }

        private string CacheBreaker
        {
            get
            {
                if (this.cacheBreaker.Length > 3)
                {
                    this.cacheBreaker = string.Empty;
                }

                return this.cacheBreaker += "\0";
            }
        }

        public void Notify(string message)
        {
            if (this.windowLocator.GetMainWindow() == null)
            {
                return;
            }

            this.snackbarMessageQueue.Enqueue(message);
        }

        public void Notify(string message, string actionLabel, Action action)
        {
            if (this.windowLocator.GetMainWindow() == null)
            {
                return;
            }

            this.snackbarMessageQueue.Enqueue(message, actionLabel, action);
        }

        public void ForceNotify(string message)
        {
            if (this.windowLocator.GetMainWindow() == null)
            {
                return;
            }

            this.snackbarMessageQueue.Enqueue(message + this.CacheBreaker, true);
        }

        public void ForceNotify(string message, string actionLabel, Action action)
        {
            if (this.windowLocator.GetMainWindow() == null)
            {
                return;
            }

            this.snackbarMessageQueue.Enqueue(message + this.CacheBreaker, actionLabel, action);
        }
    }
}
