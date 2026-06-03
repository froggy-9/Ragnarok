using System;
using System.Collections.Generic;
using UnityEngine;

namespace DeadLetterOffice.Core
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> Services = new();

        public static void Register<T>(T service) where T : class
        {
            if (service == null)
            {
                Debug.LogError($"[ServiceLocator] Cannot register null service: {typeof(T).Name}");
                return;
            }

            Services[typeof(T)] = service;
        }

        public static void Unregister<T>() where T : class
        {
            Services.Remove(typeof(T));
        }

        public static T Get<T>() where T : class
        {
            if (Services.TryGetValue(typeof(T), out object service))
            {
                return service as T;
            }

            Debug.LogError($"[ServiceLocator] Service not registered: {typeof(T).Name}");
            return null;
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            if (Services.TryGetValue(typeof(T), out object found) && found is T typedService)
            {
                service = typedService;
                return true;
            }

            service = null;
            return false;
        }

        public static void Clear()
        {
            Services.Clear();
        }
    }
}
