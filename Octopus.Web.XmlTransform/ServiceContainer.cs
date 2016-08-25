using System;
using System.Collections.Generic;

namespace Octopus.Web.XmlTransform
{
    internal class ServiceContainer : IServiceProvider, IDisposable
    {
        private readonly Dictionary<Type, object> registeredServices = new Dictionary<Type, object>();
            
        public object GetService(Type serviceType)
        {
            if (registeredServices.ContainsKey(serviceType))
                return registeredServices[serviceType];
            return null;
        }

        public void Dispose()
        {
            foreach(var implementation in registeredServices.Values)
            {
                (implementation as IDisposable)?.Dispose();
            }
            registeredServices.Clear();
        }

        public void RemoveService(Type type)
        {
            if (registeredServices.ContainsKey(type))
                registeredServices.Remove(type);
        }

        public void AddService(Type type, object implementation)
        {
            if (registeredServices.ContainsKey(type))
                throw new InvalidOperationException($"The container already contains an implementation of \'{type.FullName}'");
            registeredServices.Add(type, implementation);
        }
    }
}