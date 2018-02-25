namespace Forge.Application.Models
{
    using System.Collections;
    using System.Collections.Generic;

    public class PropertyRefreshSource : IEnumerable<string>
    {
        private readonly Model model;
        private readonly List<string> properties = new List<string>();

        public PropertyRefreshSource(Model model)
        {
            this.model = model;
        }

        public IEnumerator<string> GetEnumerator() => this.properties.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public void Add(string propertyName)
        {
            if (!this.properties.Contains(propertyName))
            {
                this.properties.Add(propertyName);
            }
        }

        public bool Remove(string propertyName) => this.properties.Remove(propertyName);

        public void Refresh()
        {
            foreach (var property in this.properties)
            {
                this.model.NotifyPropertyChanged(property);
            }
        }
    }
}
