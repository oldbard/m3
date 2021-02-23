using System;
using System.Collections.Generic;

namespace GameServices
{
    public interface IService {}

    public static class Services
    {
        static Dictionary<Type, IService> _services = new Dictionary<Type, IService>();

        static Services()
        {
            new ClientManager();
        }

        public static void RegisterService<T>(IService service) where T : IService
        {
            _services.Add(typeof(T), service);
        }

        public static T Resolve<T>() where T : IService
        {
            if (_services.TryGetValue(typeof(T), out var service))
                return (T)service;

            throw new Exception($"Could not find service of type: {typeof(T)}");
        }
    }
}