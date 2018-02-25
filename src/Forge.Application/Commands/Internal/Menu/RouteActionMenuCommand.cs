namespace Forge.Application.Commands.Internal.Menu
{
    using System;

    using Forge.Application.Routing;

    using MaterialDesignThemes.Wpf;

    internal class RouteActionMenuCommand : RouteActionCommand, IMenuCommand
    {
        public RouteActionMenuCommand(Route route, string commandText, PackIconKind? iconKind, Action execute,
            Func<bool> canExecute)
            : base(route, execute, canExecute)
        {
            this.CommandText = commandText;
            this.IconKind = iconKind;
        }

        public string CommandText { get; }

        public PackIconKind? IconKind { get; }
    }
}
