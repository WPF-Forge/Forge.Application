namespace Forge.Application.Commands.Internal.Menu
{
    using System;
    using System.Threading.Tasks;

    using Forge.Application.Routing;

    using MaterialDesignThemes.Wpf;

    internal class AsyncRouteValueMenuCommand<TParameter> : AsyncRouteValueCommand<TParameter>, IMenuCommand
        where TParameter : struct
    {
        public AsyncRouteValueMenuCommand(Route route, string commandText, PackIconKind? iconKind,
            Func<TParameter, Task> execute, Predicate<object> canExecute)
            : base(route, execute, canExecute)
        {
            this.CommandText = commandText;
            this.IconKind = iconKind;
        }

        public string CommandText { get; }

        public PackIconKind? IconKind { get; }
    }
}
