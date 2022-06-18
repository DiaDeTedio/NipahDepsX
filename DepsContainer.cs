namespace DepsX;

public abstract partial class DepsContainer
{
    readonly Dictionary<Type, Type> transients = new (32);
    readonly Dictionary<Type, Type> scoped = new (32);
    readonly Dictionary<Type, object> singletons = new (32);

    readonly Dictionary<Type, Func<object>> lazy_singletons = new (32);
    readonly Dictionary<Type, Func<object>> factories = new (32);

    internal object ExtractForScope(Type from)
    {
        if (scoped.TryGetValue(from, out Type? resolver))
            return CreateInstance(resolver);
        throw new Exception($"Scoped dependency not defined for {from.Name}");
    }

    object CreateInstance(Type type)
    {
        if (factories.TryGetValue(type, out var factory))
            return factory();

        var ctor = type.GetConstructors()[0];
        object[] args = Array.Empty<object>();
        var pars = ctor.GetParameters();
        if(pars is not null and { Length: > 0 })
        {
            args = new object[pars.Length];
            for (int i = 0; i < pars.Length; i++)
            {
                System.Reflection.ParameterInfo? param = pars[i];

                var injectType = param.ParameterType;
                var inject = Extract(injectType);
                args[i] = inject;
            }
        }

        return Activator.CreateInstance(type, args) ?? throw new Exception($"Cannot create instance of {type.Name}");
    }

    public void InjectTransient(Type from, Type to, Func<object>? factory = null)
    {
        if (factory is not null)
            factories[from] = factory;
        transients[from] = to;
    }
    public void InjectScoped(Type from, Type to, Func<object>? factory = null)
    {
        if (factory is not null)
            factories[from] = factory;
        scoped[from] = to;
    }

    public void InjectSingleton(Type from, Type to, Func<object>? factory = null)
    {
        Func<object> singletonFactory = () =>
        {
            object? singleton = factory is not null ? factory()
                        : CreateInstance(to);

            if (singleton is null)
                throw new Exception($"Cannot create singleton for {from.Name} -> {to.Name}");

            singletons[from] = singleton;
            return singleton;
        };
        lazy_singletons[from] = singletonFactory;
    }

    public void Inject(Type from, Type to, InjectionType type, Func<object>? factory = null)
    {
        if (type is InjectionType.Auto) type = InjectionType.Transient;
        switch (type)
        {
            case InjectionType.Transient:
                InjectTransient(from, to, factory);
                break;
            case InjectionType.Scoped:
                InjectScoped(from, to, factory);
                break;
            case InjectionType.Singleton:
                InjectSingleton(from, to, factory);
                break;
        }
    }

    public object Extract(Type from)
    {
        if (transients.ContainsKey(from))
            return ExtractTransient(from);
        else if (singletons.ContainsKey(from) || lazy_singletons.ContainsKey(from))
            return ExtractSingleton(from);
        else if (scoped.ContainsKey(from))
            throw new Exception("Scoped types should only be extracted within a scope context");
        else
            throw new Exception($"Dependency not injected for {from.Name}");
    }

    public object ExtractTransient(Type from)
    {
        if (transients.TryGetValue(from, out Type? to))
            return CreateInstance(to);
        throw new Exception($"Transient dependency not defined for {from.Name}");
    }

    public object ExtractSingleton(Type from)
    {
        if (singletons.TryGetValue(from, out object? singleton))
            return singleton;
        else if (lazy_singletons.TryGetValue(from, out var singletonFactory) && singletonFactory is not null)
            return singletonFactory();

        throw new Exception($"Singleton dependency not defined for {from.Name}");
    }

    public abstract void Setup();
}
public enum InjectionType
{
    /// <summary>
    /// New every time
    /// </summary>
    Transient,
    /// <summary>
    /// New for a scope
    /// </summary>
    Scoped,
    /// <summary>
    /// Only once exist for whole application
    /// </summary>
    Singleton,

    /// <summary>
    /// Injecting will fallback to Transient, (wip) extracting will get the first defined one
    /// </summary>
    Auto
}
public class DefaultDepsContainer : DepsContainer
{
    public override void Setup() { }
}