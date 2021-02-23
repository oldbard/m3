using System;
using System.Collections.Generic;

namespace GameServices
{
    public interface IService {}

    /// <summary>
    /// Basic container class that acts as a service to provide access to the main services of the game
    /// </summary>
    public static class Services
    {
        static Dictionary<Type, IService> _services = new Dictionary<Type, IService>();

        static Services()
        {
            UnityEngine.PlayerPrefs.DeleteAll();
            UnityEngine.PlayerPrefs.Save();
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