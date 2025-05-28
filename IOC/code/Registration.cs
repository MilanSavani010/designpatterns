using System.Collections.Concurrent;
using System.Reflection;

namespace ProductService.IOC;


public enum Lifetime
{
    Transient,
    Singleton,
    Scoped
}

[AttributeUsage(AttributeTargets.Property)]
public class InjectAttribute : Attribute
{
    public string Name { get; }
    public InjectAttribute(string name = "")
    {
        Name = name;
    }
}

[AttributeUsage(AttributeTargets.Constructor)]
public class InjectConstructorAttribute : Attribute { }

public class Registration
{
    public Type ImplementationType { get; }
    public Lifetime Lifetime { get; }
    public object Instance { get; set; }
    public Func<IOCContainer, object> Factory { get; }

    private object _singletonInstance;
    private object _scopedInstance;
    private readonly object _singletonLock = new();
    private bool IsOpenGeneric => ImplementationType.IsGenericTypeDefinition;


    public Registration(Type implementationType, Lifetime lifetime, Func<IOCContainer, object> factory = null, Interceptor interceptor = null)
    {
        ImplementationType = implementationType;
        Lifetime = lifetime;
        Factory = factory;
    }

    public object GetInstance(IOCContainer container, bool isScoped, Type[] genericArguments = null)
    {
        if(IsOpenGeneric)
        {
            if(genericArguments==null || genericArguments.Length == 0)
            {
                throw new Exception($"Open generic type {ImplementationType.Name} requires type parameters.");
            }
            var closedType = ImplementationType.MakeGenericType(genericArguments);
            return Activator.CreateInstance(closedType);
        }

        if (Lifetime == Lifetime.Singleton)
        {
            if (_singletonInstance == null)
            {
                lock (_singletonLock)  // Ensure only one thread initializes the singleton
                {
                    if (_singletonInstance == null)
                    {
                        _singletonInstance = CreateInstance(container);
                    }
                }
            }
            return _singletonInstance;
        }
        if (Lifetime == Lifetime.Scoped && isScoped)
        {
            return _scopedInstance ??= CreateInstance(container);
        }
        return CreateInstance(container);
    }

    private object CreateInstance(IOCContainer container)
    {
        if (container == null) throw new InvalidOperationException("IoCContainer instance is required for resolution.");

        object instance = Factory?.Invoke(container) ?? CreateInstanceWithConstructorInjection(container);
      
        
        InjectProperties(instance, container);
        return instance;

    }

    private object CreateInstanceWithConstructorInjection(IOCContainer container)
    {
        var constructor = ImplementationType.GetConstructors()
            .Where(c => c.GetParameters().All(p=>!p.ParameterType.IsPrimitive))
            .OrderByDescending(c=>c.GetParameters().Count())
            .FirstOrDefault();

        if (constructor == null)
        {
            throw new Exception($"No public constructor found for {ImplementationType.Name}");

        }
        var parameters = constructor.GetParameters()
          .Select(p => IsPrimitiveOrString(p.ParameterType) ? GetDefaultValue(p.ParameterType) : container.Resolve(p.ParameterType))
          .ToArray();

        return constructor.Invoke(parameters);

    }

    private static bool IsPrimitiveOrString(Type type)
    {
        return type.IsPrimitive || type == typeof(string) || type == typeof(decimal);
    }

    private object? GetDefaultValue(Type type)
    {
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }

    private void InjectProperties(object instance, IOCContainer container)
    {
        foreach (var prop in ImplementationType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                 .Where(p => p.CanWrite && p.GetCustomAttribute<InjectAttribute>() != null))
        {
            var injectAttr = prop.GetCustomAttribute<InjectAttribute>();
            object dependency = container.Resolve(prop.PropertyType, injectAttr.Name);
            if (dependency == null)
            {
                throw new Exception($"Failed to resolve dependency for {prop.Name} in {ImplementationType.Name}");
            }
            prop.SetValue(instance, dependency);
        }
    }
}

public class RegistrationStore 
{
    private readonly ConcurrentDictionary<(Type, string), Registration> _registrations = new ConcurrentDictionary<(Type, string), Registration>();
    public void Add(Type serviceType, string name, Registration registration)
    {
        _registrations[(serviceType, name)] = registration;
    }

  
    public bool TryGet(Type serviceType, string name, out Registration registration)
    {
        return _registrations.TryGetValue((serviceType, name), out registration);
    }

   


}



