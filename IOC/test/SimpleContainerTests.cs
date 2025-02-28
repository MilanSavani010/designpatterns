using System.ComponentModel;
using System.Reflection;

[TestFixture]
public class IoCContainerTests
{
    private IOCContainer _container;
    private RegistrationStore _store;

    [SetUp]
    public void SetUp()
    {
        _store = new RegistrationStore();
        _container = new IOCContainer(_store);
    }

    [Test]
    public void RegisterAndResolve_TransientService_ShouldCreateNewInstance()
    {
        _container.Register<IService, ServiceImplementation>(lifetime: Lifetime.Transient);
        var instance1 = _container.Resolve<IService>();
        var instance2 = _container.Resolve<IService>();
        Assert.AreNotSame(instance1, instance2);
    }

    [Test]
    public void RegisterAndResolve_SingletonService_ShouldReturnSameInstance()
    {
        _container.Register<IService, ServiceImplementation>(lifetime: Lifetime.Singleton);
        var instance1 = _container.Resolve<IService>();
        var instance2 = _container.Resolve<IService>();
        Assert.AreSame(instance1, instance2);
    }

    [Test]
    public void RegisterAndResolve_ScopedService_ShouldReturnSameInstanceWithinScope()
    {
        _container.Register<IService, ServiceImplementation>(lifetime: Lifetime.Scoped);
        using (var scope = _container.CreateScope())
        {
            var instance1 = scope.Resolve<IService>();
            var instance2 = scope.Resolve<IService>();
            Assert.AreSame(instance1, instance2);
        }
    }

    [Test]
    public void RegisterAndResolve_WithFactory_ShouldUseFactoryMethod()
    {
        _container.Register<IService>(c => new ServiceImplementation(), lifetime: Lifetime.Transient);
        var instance = _container.Resolve<IService>();
        Assert.IsNotNull(instance);
    }



    [Test]
    public void RegisterAndResolve_WithOpenGenericType_ShouldUseOpeneGenericType()
    {
        _container.Register(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        var userRepository = _container.Resolve<IGenericRepository<A>>();
        Assert.IsNotNull(userRepository);
    }

    [Test]
    public void RegisterAndResolve_NamedRegistration_ShouldReturnCorrectInstance()
    {
        _container.Register<IService, FirstService>("First");
        _container.Register<IService, SecondService>("Second");
        var firstService = _container.Resolve<IService>("First");
        var secondService = _container.Resolve<IService>("Second");
        Assert.IsInstanceOf<FirstService>(firstService);
        Assert.IsInstanceOf<SecondService>(secondService);
    }

    [Test]
    public void RegisterModule_ShouldRegisterServicesFromModule()
    {
        var module = new TestModule();
        _container.RegisterModule(module);
        var service = _container.Resolve<IService>();
        Assert.IsNotNull(service);
    }

    [Test]
    public void AutoRegister_ShouldRegisterAllTypesInAssembly()
    {
        _container.AutoRegister(Assembly.GetExecutingAssembly());
        var service = _container.Resolve<IService>();
        Assert.IsNotNull(service);
    }

    [Test]
    public void CircularDependency_ShouldThrowException()
    {
        _container.Register<A, A>();
        _container.Register<B, B>();
        Assert.Throws<Exception>(() => _container.Resolve<A>());
    }

    [Test]
    public void PropertyInjection_ShouldInjectDependencies()
    {
        _container.Register<IDependency, DependencyImplementation>();
        _container.Register<IService, ServiceWithPropertyInjection>();
        var service = _container.Resolve<IService>() as ServiceWithPropertyInjection;
        Assert.IsNotNull(service?.Dependency);
    }

    [Test]
    public void ConstructorInjection_ShouldUseInjectConstructorAttribute()
    {
        _container.Register<IDependency, DependencyImplementation>();
        _container.Register<IService, ServiceWithMultipleConstructors>();
        var service = _container.Resolve<IService>() as ServiceWithMultipleConstructors;
        Assert.IsNotNull(service);
        Assert.That(service.SelectedConstructor, Is.EqualTo("InjectConstructor"));
    }

    [Test]
    public void ConstructorInjection_ShouldUseConstructorWithMostParametersIfNoInjectAttribute()
    {
        _container.Register<IDependency, DependencyImplementation>();
        _container.Register<IService, ServiceWithTwoConstructorsWithoutInject>();
        var service = _container.Resolve<IService>() as ServiceWithTwoConstructorsWithoutInject;
        Assert.IsNotNull(service);
        Assert.That(service.SelectedConstructor, Is.EqualTo("MostParameters"));
    }


    [Test]
    public void ConditionalRegistration_ShouldRegisterOnlyIfConditionMet()
    {
        bool condition = true;

        if (condition)
        {
            _container.Register<IService, ServiceImplementation>();
        }

        var service = _container.Resolve<IService>();
        Assert.IsNotNull(service);
        Assert.IsInstanceOf<ServiceImplementation>(service);
    }

    [Test]
    public void ConditionalRegistration_ShouldNotRegisterIfConditionNotMet()
    {
        bool condition = false;

        if (condition)
        {
            _container.Register<IService, ServiceImplementation>();
        }

        Assert.Throws<Exception>(() => _container.Resolve<IService>());
    }

    [Test]
    public void ConditionalRegistration_ShouldAllowMultipleConditionalBindings()
    {
        bool condition1 = true;
        bool condition2 = false;

        if (condition1)
        {
            _container.Register<IService, ServiceImplementation>();
        }

        if (condition2)
        {
            _container.Register<IService, AlternativeServiceImplementation>();
        }

        var service = _container.Resolve<IService>();
        Assert.IsNotNull(service);
        Assert.IsInstanceOf<ServiceImplementation>(service);
    }

    [Test]
    public void ParallelResolution_ShouldBeThreadSafe()
    {
        var container = new IOCContainer(new RegistrationStore());
        container.Register<IService, FirstService>("FirstService", lifetime: Lifetime.Singleton);
        container.Register<IService, SecondService>("SecondService", lifetime: Lifetime.Transient);

        IService[] Signletonresults = new IService[1000];
        IService[] Transientresults = new IService[1000];

        Parallel.For(0, 1000, i =>
        {
            Signletonresults[i] = container.Resolve<IService>("FirstService");
            Transientresults[i] = container.Resolve<IService>("SecondService");


        });

        Assert.That(Signletonresults.All(r => r == Signletonresults[0]), "All instances should be the same singleton instance.");
        Assert.That(Transientresults.Distinct().Count(), Is.EqualTo(1000), "Each transient instance must be unique.");
    }

    [Test]
    public void InterceptionWithDynamicProxy_ShouldLog()
    {
        IOCContainer container = new IOCContainer(new RegistrationStore());
        container.Register<IInterceptedService, InterceptedService>();


        // Register interceptors
        container.RegisterInterceptor<IInterceptedService>(new LoggingInterceptor());
        container.RegisterInterceptor<IInterceptedService>(new TimingInterceptor());

        var interceptedService = container.Resolve<IInterceptedService>();

        string message = interceptedService.SayHello("Milan");
        Console.WriteLine(message);


    }

    [Test]
    public void Interceptor_Should_Log_Method_Calls()
    {
        // Arrange
        _container.Register<IInterceptedService, InterceptedService>();
        _container.RegisterInterceptor<IInterceptedService>(new LoggingInterceptor());

        var service = _container.Resolve<IInterceptedService>();

        // Act
        string result = service.SayHello("Milan");

        // Assert
        Assert.AreEqual("Hello, Milan!", result);
    }



}

// Supporting Classes for Testing
public interface IService { }
public class ServiceImplementation : IService { }
public class FirstService : IService { }
public class SecondService : IService { }
public interface IDependency { }
public class DependencyImplementation : IDependency { }
public class ServiceWithPropertyInjection : IService
{
    [Inject]
    public IDependency Dependency { get; set; }
}
public class TestModule : IModule
{
    public void Register(IOCContainer container)
    {
        container.Register<IService, ServiceImplementation>();
    }
}
public class A { public A(B b) { } }
public class B { public B(A a) { } }

public class ServiceWithMultipleConstructors : IService
{
    public string SelectedConstructor { get; }

    public ServiceWithMultipleConstructors()
    {
        SelectedConstructor = "Default";
    }

    [InjectConstructor]
    public ServiceWithMultipleConstructors(IDependency dependency)
    {
        SelectedConstructor = "InjectConstructor";
    }
}

public class ServiceWithTwoConstructorsWithoutInject : IService
{
    public string SelectedConstructor { get; }

    public ServiceWithTwoConstructorsWithoutInject()
    {
        SelectedConstructor = "Default";
    }

    public ServiceWithTwoConstructorsWithoutInject(IDependency dependency, string extraParam)
    {
        SelectedConstructor = "MostParameters";
    }
}


public class AlternativeServiceImplementation : IService { }

public interface IGenericRepository<T>
{

}

public class GenericRepository<T> : IGenericRepository<T>
{
    public GenericRepository()
    {
        
    }
}

public interface IInterceptedService
{
    public string SayHello(string name);
}

public class InterceptedService: IInterceptedService
{
    public string SayHello(string name) => $"Hello, {name}!";

}