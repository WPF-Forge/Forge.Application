namespace Forge.Application.Commands
{
    using MaterialDesignThemes.Wpf;

    public interface IMenuCommand : IRefreshableCommand
    {
        string CommandText { get; }

        PackIconKind? IconKind { get; }
    }
}
