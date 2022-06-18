using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DepsX;

public static class Deps
{
    static DepsContainer def = new DefaultDepsContainer();
    static Dictionary<Type, DepsContainer> containers = new(32);

    static DepsContainer GetContainer(Type type)
    {
        if (type.IsAssignableFrom(typeof(DepsContainer)))
            throw new Exception($"Cannot get a container from a non-container type {type.Name}");

        if (containers.TryGetValue(type, out var container))
            return container;
        container = (DepsContainer?)Activator.CreateInstance(type);
        if (container is null)
            throw new Exception($"Cannot create container instance for {type.Name}");
        container.Setup();
        return containers[type] = container;
    }

    public static DepsContainer On(Type containerType) => GetContainer(containerType);
    public static TContainer On<TContainer>() where TContainer : DepsContainer 
        => (TContainer)GetContainer(typeof(TContainer));

    public static void InjectTransient(Type from, Type to, Func<object>? factory = null)
        => def.InjectTransient(from, to, factory);
    public static void InjectScoped(Type from, Type to, Func<object>? factory = null)
        => def.InjectScoped(from, to, factory);
    public static void InjectSingleton(Type from, Type to, Func<object>? factory = null)
        => def.InjectSingleton(from, to, factory);

    public static void InjectTransient<TService, TImplementation>(Func<TImplementation>? factory = null)
        => def.InjectTransient<TService, TImplementation>(factory);
    public static void InjectScoped<TService, TImplementation>(Func<TImplementation>? factory = null)
        => def.InjectScoped<TService, TImplementation>(factory);
    public static void InjectSingleton<TService, TImplementation>(Func<TImplementation>? factory = null)
        => def.InjectSingleton<TService, TImplementation>(factory);

    internal static object ExtractForScope(Type from) => def.ExtractForScope(from);

    public static object Extract(Type from) => def.Extract(from);
    public static T Extract<T>() => def.Extract<T>();

    public static object ExtractTransient(Type from) => def.ExtractTransient(from);
    public static T ExtractTransient<T>() => def.ExtractTransient<T>();

    public static object ExtractSingleton(Type from) => def.ExtractSingleton(from);
    public static T ExtractSingleton<T>() => def.ExtractSingleton<T>();
}
