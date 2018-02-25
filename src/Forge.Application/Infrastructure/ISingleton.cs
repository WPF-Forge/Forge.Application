namespace Forge.Application.Infrastructure
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    using Forge.Application.Properties;

    public interface ISingleton<T> where T : class
    {
        T Instance { get; set; }
    }

    public class Singleton<T> : ISingleton<T>, INotifyPropertyChanged where T : class
    {
        private T instance;

        public Singleton(T instance)
        {
            this.Instance = instance;
        }

        public T Instance
        {
            get { return this.instance; }
            set
            {
                if (Equals(value, this.instance)) return;
                this.instance = value;
                this.OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
