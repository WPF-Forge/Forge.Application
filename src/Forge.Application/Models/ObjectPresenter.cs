namespace Forge.Application.Models
{
    public class ObjectPresenter
    {
        public ObjectPresenter(object instance, string displayString)
        {
            this.Object = instance;
            this.DisplayString = displayString;
        }

        public object Object { get; }

        public string DisplayString { get; }

        public override string ToString() => this.DisplayString;
    }

    public class ObjectPresenter<T> : ObjectPresenter
    {
        public ObjectPresenter(T instance, string displayString)
            : base(instance, displayString)
        {
            this.Object = instance;
        }

        public new T Object { get; }
    }
}
