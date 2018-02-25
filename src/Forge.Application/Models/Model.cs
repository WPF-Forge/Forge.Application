namespace Forge.Application.Models
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    using Forge.Application.Properties;

    public abstract class Model : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private readonly Dictionary<string, List<string>> errors = new Dictionary<string, List<string>>();

        public virtual bool IsValid => !this.HasErrors;

        public bool HasErrors => this.errors.Count != 0;

        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return null;
            }

            List<string> errorList;
            this.errors.TryGetValue(propertyName, out errorList);
            return errorList;
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool AddError(string error, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
            {
                return true;
            }

            this.errors[propertyName] = new List<string> { error };
            this.NotifyErrorsChanged(propertyName);
            return true;
        }

        public bool RemoveError([CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
            {
                return true;
            }

            if (this.errors.Remove(propertyName))
            {
                this.NotifyErrorsChanged(propertyName);
            }

            return true;
        }

        protected bool ValidateProperty(bool isValid, string error, [CallerMemberName] string propertyName = null)
        {
            if (isValid)
            {
                this.RemoveError(propertyName);
            }
            else
            {
                this.AddError(error, propertyName);
            }

            return isValid;
        }

        public void ClearErrors() => this.errors.Clear();

        public virtual bool Validate() => this.IsValid;

        public virtual void NotifyErrorsChanged(string propertyName)
            => this.ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));

        protected RefreshSource RefreshSource() => new RefreshSource(this);

        [NotifyPropertyChangedInvocator]
        public virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
