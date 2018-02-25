namespace Forge.Application.Demo.Routes
{
    using System.Windows.Input;

    using Forge.Application.Routing;

    public class ContactRoute : Route
    {
        private string name;
        private string message;

        public ContactRoute(string name)
        {
            this.RouteConfig.Title = "Contact us";

            // We do something with the passed argument.
            this.Name = name;

            // Initialize commands.
            this.CancelCommand = this.Command(this.Cancel);
            this.SendCommand = this.Command(this.Send);
        }

        public string Name
        {
            get => this.name;
            set
            {
                if (Equals(this.name, value)) return;
                this.name = value;
                this.NotifyPropertyChanged();
                if (string.IsNullOrWhiteSpace(this.name))
                {
                    this.AddError("Your name is required.");
                }
                else
                {
                    this.RemoveError();
                }
            }
        }

        public string Message
        {
            get => this.message;
            set
            {
                if (Equals(this.message, value)) return;
                this.message = value;
                this.NotifyPropertyChanged();
                if (this.message == null || this.message.Length < 10)
                {
                    this.AddError("Message must contain at least 10 characters.");
                }
                else
                {
                    this.RemoveError();
                }
            }
        }

        public ICommand CancelCommand { get; set; }

        public ICommand SendCommand { get; set; }

        private void Cancel()
        {
            // Return false to indicate that we canceled.
            this.PopRoute(false);
        }

        private void Send()
        {
            if (!this.Validate())
            {
                return;
            }

            // Send email here...
            var emailName = this.Name;
            var emailMessage = this.Message;

            // Pop and return true to indicate that the email is sent.
            this.PopRoute(true);
        }

        private bool Validate()
        {
            if (string.IsNullOrWhiteSpace(this.name))
            {
                this.AddError("Your name is required.", nameof(this.Name));
                return false;
            }

            if (this.message == null || this.message.Length < 10)
            {
                this.AddError("Message must contain at least 10 characters.", nameof(this.Message));
                return false;
            }

            return true;
        }
    }
}
