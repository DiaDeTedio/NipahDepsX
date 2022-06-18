namespace DepsX;

public abstract partial class DepsContainer
{
    public void InjectTransient<TService, TImplementation>(Func<TImplementation>? factory = null)
        => InjectTransient(typeof(TService), typeof(TImplementation),
            factory is not null ? () => factory()! : null);

    public void InjectScoped<TService, TImplementation>(Func<TImplementation>? factory = null)
        => InjectScoped(typeof(TService), typeof(TImplementation),
            factory is not null ? () => factory()! : null);

    public void InjectSingleton<TService, TImplementation>(Func<TImplementation>? factory = null)
        => InjectSingleton(typeof(TService), typeof(TImplementation),
            factory is not null ? () => factory()! : null);

    public T Extract<T>() => (T)Extract(typeof(T));
    public T ExtractTransient<T>() => (T)ExtractTransient(typeof(T));
    public T ExtractSingleton<T>() => (T)ExtractSingleton(typeof(T));
}