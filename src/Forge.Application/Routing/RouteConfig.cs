namespace Forge.Application.Routing
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    using Forge.Application.Commands;
    using Forge.Application.Properties;

    using MaterialDesignThemes.Wpf;

    public class RouteConfig : INotifyPropertyChanged
    {
        private PackIconKind? icon;
        private List<KeyBinding> keyBindings;
        private bool showAppBar = true;
        private bool showTitle = true;
        private string title;

        public RouteConfig()
        {
            this.RouteCommands = new ObservableCollection<IMenuCommand>();
            this.KeyBindings = new List<KeyBinding>();
        }

        public string Title
        {
            get { return this.title; }
            set
            {
                if (value == this.title) return;
                this.title = value;
                this.OnPropertyChanged();
            }
        }

        public bool ShowTitle
        {
            get { return this.showTitle; }
            set
            {
                if (value == this.showTitle) return;
                this.showTitle = value;
                this.OnPropertyChanged();
            }
        }

        public bool ShowAppBar
        {
            get { return this.showAppBar; }
            set
            {
                if (value == this.showAppBar) return;
                this.showAppBar = value;
                this.OnPropertyChanged();
            }
        }

        public PackIconKind? Icon
        {
            get { return this.icon; }
            set
            {
                if (value == this.icon) return;
                this.icon = value;
                this.OnPropertyChanged();
            }
        }

        public ObservableCollection<IMenuCommand> RouteCommands { get; }

        public List<KeyBinding> KeyBindings
        {
            get { return this.keyBindings; }
            set
            {
                if (Equals(value, this.keyBindings)) return;
                this.keyBindings = value;
                this.OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void AddRouteCommandsSeparator() => this.RouteCommands.Add(null);

        public void RefreshKeyBindings() => this.OnPropertyChanged(nameof(this.KeyBindings));

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
