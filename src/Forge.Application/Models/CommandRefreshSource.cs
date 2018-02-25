namespace Forge.Application.Models
{
    using System.Collections;
    using System.Collections.Generic;

    using Forge.Application.Commands;

    public class CommandRefreshSource : IEnumerable<IRefreshableCommand>
    {
        private readonly List<IRefreshableCommand> commands = new List<IRefreshableCommand>();

        public IEnumerator<IRefreshableCommand> GetEnumerator() => this.commands.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public void Add(IRefreshableCommand command)
        {
            if (command != null && !this.commands.Contains(command))
            {
                this.commands.Add(command);
            }
        }

        public bool Remove(IRefreshableCommand command) => this.commands.Remove(command);

        public void Refresh()
        {
            foreach (var command in this.commands)
            {
                command.RaiseCanExecuteChanged();
            }
        }
    }
}
