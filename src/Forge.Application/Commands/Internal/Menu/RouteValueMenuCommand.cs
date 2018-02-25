namespace Forge.Application.Commands.Internal.Menu
{
    using System;

    using Forge.Application.Routing;

    using MaterialDesignThemes.Wpf;

    internal class RouteValueMenuCommand<TParameter> : RouteValueCommand<TParameter>, IMenuCommand
        where TParameter : struct
    {
        public RouteValueMenuCommand(Route route, string commandText, PackIconKind? iconKind, Action<TParameter> execute,
            Predicate<object> canExecute)
            : base(route, execute, canExecute)
        {
            this.CommandText = commandText;
            this.IconKind = iconKind;
        }

        public string CommandText { get; }

        public PackIconKind? IconKind { get; }
    }
}
