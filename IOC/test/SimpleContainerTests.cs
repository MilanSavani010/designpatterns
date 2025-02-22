using NUnit.Framework;
using ProductService.IOC;
using System;
using System.Collections.Generic;
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
        _container.Register<IService>(c => new ServiceImplementation(), lifetime : Lifetime.Transient) ;
        var instance = _container.Resolve<IService>();
        Assert.IsNotNull(instance);
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