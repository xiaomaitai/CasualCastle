using System;
using System.Collections.Generic;

namespace CasualCastle.Adapters.Godot;

public static class AdapterRegistry
{
    private static readonly Dictionary<Type, object> _instances = new();

    public static void Register<T>(T instance) where T : class
    {
        _instances[typeof(T)] = instance;
    }

    public static T Resolve<T>() where T : class
    {
        return _instances.TryGetValue(typeof(T), out object instance) ? (T)instance : null;
    }

    public static void Unregister<T>(T instance) where T : class
    {
        if (_instances.TryGetValue(typeof(T), out object existing) && existing == instance)
            _instances.Remove(typeof(T));
    }
}
