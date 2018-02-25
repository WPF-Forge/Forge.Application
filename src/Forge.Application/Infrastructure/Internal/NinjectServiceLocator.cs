namespace Forge.Application.Infrastructure.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Forge.Application.Helpers.Internal;

    using Ninject;

    internal class NinjectServiceLocator : IServiceLocator
    {
        private readonly IKernel kernel;

        public NinjectServiceLocator(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public T Get<T>(IDictionary<string, object> parameters) => parameters == null
            ? this.kernel.Get<T>()
            : this.kernel.Get<T>(parameters.Select(IocHelpers.MapParameter).ToArray());

        public object Get(Type type, IDictionary<string, object> parameters) => parameters == null
            ? this.kernel.Get(type)
            : this.kernel.Get(type, parameters.Select(IocHelpers.MapParameter).ToArray());
    }
}
