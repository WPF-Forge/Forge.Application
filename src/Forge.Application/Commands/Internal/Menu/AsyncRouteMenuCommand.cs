namespace Forge.Application.Commands.Internal.Menu
{
    using System;
    using System.Threading.Tasks;

    using Forge.Application.Routing;

    using MaterialDesignThemes.Wpf;

    internal class AsyncRouteMenuCommand<TParameter> : AsyncRouteCommand<TParameter>, IMenuCommand
        where TParameter : class
    {
        public AsyncRouteMenuCommand(Route route, string commandText, PackIconKind? iconKind,
            Func<TParameter, Task> execute, Predicate<TParameter> canExecute, bool ignoreNullParameters)
            : base(route, execute, canExecute, ignoreNullParameters)
        {
            this.CommandText = commandText;
            this.IconKind = iconKind;
        }

        public string CommandText { get; }

        public PackIconKind? IconKind { get; }
    }
}
