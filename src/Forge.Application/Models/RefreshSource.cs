namespace Forge.Application.Models
{
    using Forge.Application.Commands;

    public class RefreshSource
    {
        public RefreshSource(Model model)
        {
            this.Properties = new PropertyRefreshSource(model);
            this.Commands = new CommandRefreshSource();
        }

        public PropertyRefreshSource Properties { get; }

        public CommandRefreshSource Commands { get; }

        public void Refresh()
        {
            this.Properties.Refresh();
            this.Commands.Refresh();
        }

        public RefreshSource WithProperties(params string[] properties)
        {
            if (properties != null)
            {
                foreach (var property in properties)
                {
                    this.Properties.Add(property);
                }
            }

            return this;
        }

        public RefreshSource WithCommands(params IRefreshableCommand[] commands)
        {
            if (commands != null)
            {
                foreach (var command in commands)
                {
                    this.Commands.Add(command);
                }
            }

            return this;
        }
    }
}
