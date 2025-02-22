using System.Reflection;

namespace ProductService.IOC;
public interface IModule
{
    void Register(IOCContainer container);
}
public class IOCContainer : IDisposable
{
    public readonly RegistrationStore _store;
    private readonly ThreadLocal<Dictionary<Type, object>> _scopedInstances = new(() => new Dictionary<Type, object>());
    private readonly HashSet<Type> _resolving = new HashSet<Type>();
    private readonly object _lock = new object();
    private readonly bool _isScope;


    public IOCContainer(RegistrationStore store, bool isScope = false)
    {
        _store = store;
        _isScope = isScope;
    }

    public IOCContainer CreateScope()
    {
        return new IOCContainer(_store, true);
    }

    public void Register<TInterface, TImplementation>(string name = "", Lifetime lifetime = Lifetime.Transient)
        where TImplementation : TInterface
    {

        _store.Add(typeof(TInterface), name, new Registration(typeof(TImplementation), lifetime));
    }

    public void Register<TInterface>(Func<IOCContainer, TInterface> factory, string name = "", Lifetime lifetime = Lifetime.Transient)
    {
        _store.Add(typeof(TInterface), name, new Registration(typeof(TInterface), lifetime, c => factory(this)));
    }

    public void Register(Type interfaceType, Type implementationType, Lifetime lifetime = Lifetime.Transient)
    {
        _store.Add(interfaceType, "", new Registration(implementationType, lifetime));
    }


    public void RegisterIf<TInterface,TImplementation>(Func<bool> condition,string name ="",Lifetime lifetime = Lifetime.Transient) where TImplementation : TInterface
    {
        if(condition())
        {
            Register<TInterface,TImplementation>(name, lifetime);
        }
    }

    public void RegisterIf(Type interfaceType,Type implementationType,Func<bool> condition,Lifetime lifetime = Lifetime.Transient)
    {
        if(condition())
        {
            Register(interfaceType, implementationType,lifetime);
        }
    }

    public void RegisterModule(IModule module)
    {
        module.Register(this);
    }

    

    public object Resolve(Type type, string name = "")
    {

        return ResolveInternal(type, name);
    }

    public T Resolve<T>(string name = "")
    {
        return (T)Resolve(typeof(T), name);
    }

    private object ResolveInternal(Type type, string name)
    {

        lock (_lock)
        {
            if (!_store.TryGet(type, name, out Registration registration))
            {
                throw new Exception($"Type {type.Name} not registered with name '{name}'");
            }

            if (_resolving.Contains(type))
            {
                throw new Exception($"Circular dependency detected: {type.Name}");
            }

            _resolving.Add(type);

          

            if (registration.Lifetime == Lifetime.Scoped && _isScope)
            {
                if (!_scopedInstances.Value.ContainsKey(type))
                {
                    _scopedInstances.Value[type] = registration.GetInstance(this, true);
                }
                _resolving.Remove(type);
                return _scopedInstances.Value[type];
            }

            object instance = registration.GetInstance(this, _scopedInstances.Value.ContainsKey(type));
            _resolving.Remove(type);
            return instance;
        }
    }

    public void AutoRegister(Assembly assembly)
    {
        var types = assembly.GetTypes();
        foreach (var type in types)
        {
            var interfaces = type.GetInterfaces();
            foreach (var iface in interfaces)
            {
                Register(iface, type, Lifetime.Transient);
            }
        }
    }

    public void Dispose()
    {
    }
}


