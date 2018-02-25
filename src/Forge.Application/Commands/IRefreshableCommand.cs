namespace Forge.Application.Commands
{
    using System.Windows.Input;

    public interface IRefreshableCommand : ICommand
    {
        void RaiseCanExecuteChanged();
    }
}
