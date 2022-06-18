namespace WiProtect.App.DepsX;

public class Scope : IDisposable
{
    Dictionary<Type, object> scoped = new (32);
    Type? container;

    public object Extract(Type from)
    {
        if (scoped.TryGetValue(from, out object? instance))
            return instance;

        if (container is not null)
            instance = Deps.On(container).ExtractForScope(from);
        else
            instance = Deps.ExtractForScope(from);
        scoped[from] = instance;

        return instance;
    }

    public T Extract<T>() => (T)Extract(typeof(T));

    public static Scope On(Type container)
    {
        if (container.IsAssignableTo(typeof(DepsContainer)) is false)
            throw new Exception($"{container.Name} should implement {typeof(DepsContainer).Name}");
        return new Scope { container = container };
    }

    public static Scope On<TContainer>() where TContainer : DepsContainer
        => new Scope { container = typeof(TContainer) };

    public void Dispose()
    {
        scoped.Clear();
    }
}