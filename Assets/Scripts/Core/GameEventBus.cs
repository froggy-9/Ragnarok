using System;
using System.Collections.Generic;

namespace DeadLetterOffice.Core
{
    public static class GameEventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> Handlers = new();

        public static void Subscribe<T>(Action<T> handler)
        {
            if (handler == null)
            {
                return;
            }

            Type type = typeof(T);
            if (!Handlers.TryGetValue(type, out List<Delegate> handlers))
            {
                handlers = new List<Delegate>();
                Handlers[type] = handlers;
            }

            if (!handlers.Contains(handler))
            {
                handlers.Add(handler);
            }
        }

        public static void Unsubscribe<T>(Action<T> handler)
        {
            if (handler == null)
            {
                return;
            }

            if (Handlers.TryGetValue(typeof(T), out List<Delegate> handlers))
            {
                handlers.Remove(handler);
            }
        }

        public static void Publish<T>(T evt)
        {
            if (!Handlers.TryGetValue(typeof(T), out List<Delegate> handlers))
            {
                return;
            }

            List<Delegate> snapshot = new(handlers);
            foreach (Delegate handler in snapshot)
            {
                if (handler is Action<T> typedHandler)
                {
                    typedHandler.Invoke(evt);
                }
            }
        }

        public static void Clear()
        {
            Handlers.Clear();
        }
    }
}
