# IoC Container Documentation & User Guide

---

## Table of Contents

1. [Introduction](#introduction)
2. [Features Overview](#features-overview)
   - [Core Features](#core-features)
   - [Advanced Features](#advanced-features)
3. [Installation](#installation)
4. [Registering Dependencies](#registering-dependencies)
5. [Resolving Dependencies](#resolving-dependencies)
6. [Advanced Usage](#advanced-usage)
7. [Custom Injection Attributes](#custom-injection-attributes)
8. [Constructor Selection Strategy](#constructor-selection-strategy)
9. [Testing](#testing)
10. [Examples](#examples)

---

## Introduction

The IoC (Inversion of Control) Container is a lightweight, feature-rich dependency injection framework for .NET. It provides flexible object creation, lifetime management, and dependency resolution, supporting both basic and advanced scenarios for modular and testable applications.

---

## Features Overview

### Core Features

1. **Basic Registration and Resolution**  
   Register and resolve interfaces and their implementations.

2. **Constructor Injection**  
   Automatically resolves dependencies via constructors, selecting the most suitable one.

3. **Property Injection**  
   Inject dependencies into properties using the `[Inject]` attribute.

4. **Lifetimes**  
   - **Singleton:** One instance per container.
   - **Scoped:** One instance per scope.
   - **Transient:** New instance each time resolved.

5. **Named Registrations**  
   Register multiple implementations for the same interface using a name identifier.

---

### Advanced Features

6. **Automatic Registration (Assembly Scanning)**  
   Register all types implementing an interface within an assembly.

7. **Open Generic Type Support**  
   Register and resolve open generic types dynamically.

8. **Conditional Registration**  
   Register implementations based on runtime conditions.

9. **Module-Based Registration**  
   Encapsulate multiple registrations in modules for better organization.

10. **Lazy Dependency Resolution**  
    Resolve dependencies on-demand without premature instantiation.

11. **Thread Safety**  
    Safe for concurrent resolutions using synchronization.

12. **Constructor Selection Strategy**  
    Automatically selects the most suitable constructor for dependency injection.

13. **Circular Dependency Detection**  
    Prevents infinite loops caused by circular references in dependencies.

14. **Scoped Resolution**  
    Ensures dependencies are resolved within a controlled lifetime scope.

15. **Factory-Based Registration**  
    Register dependencies using factory functions.

16. **Primitive Type Handling**  
    Ignores primitive types when resolving constructor dependencies.

17. **Custom Attribute Support**  
    Use `[Inject]` and `[InjectConstructor]` attributes to mark properties and constructors for injection.

18. **Extensibility & Customization**  
    Easily extend or modify the container for specific project needs.

---

## Installation

Add the required source files to your project and reference the namespace:

```csharp
using ProductService.IOC;
```

---

## Registering Dependencies

### Basic Registration

```csharp
var container = new IOCContainer(new RegistrationStore());
container.Register<IService, ServiceImplementation>();
```

### Specifying Lifetime

```csharp
container.Register<IService, ServiceImplementation>(lifetime: Lifetime.Singleton);
```

- **Transient:** New instance per resolution.
- **Singleton:** Single instance for the container.
- **Scoped:** Single instance per scope.

### Factory-Based Registration

```csharp
container.Register<IService>(c => new ServiceImplementation(), lifetime: Lifetime.Singleton);
```

### Named Registration

```csharp
container.Register<IService, ServiceA>("ServiceA");
container.Register<IService, ServiceB>("ServiceB");
```

---

## Resolving Dependencies

### Resolving a Type

```csharp
var service = container.Resolve<IService>();
```

### Resolving Named Instances

```csharp
var serviceA = container.Resolve<IService>("ServiceA");
var serviceB = container.Resolve<IService>("ServiceB");
```

### Constructor Injection

```csharp
public class Consumer
{
    private readonly IService _service;
    public Consumer(IService service)
    {
        _service = service;
    }
}
```

### Property Injection

```csharp
public class Consumer
{
    [Inject]
    public IService Service { get; set; }
}
```

---

## Advanced Usage

### Open Generic Type Support

```csharp
container.Register(typeof(IRepository<>), typeof(Repository<>));
var userRepository = container.Resolve<IRepository<User>>();
```

### Conditional Registration

```csharp
container.RegisterIf<IService, SpecialService>(() => DateTime.Now.Hour < 12);
```

### Module-Based Registration

```csharp
public class MyModule : IModule
{
    public void Register(IOCContainer container)
    {
        container.Register<IService, ServiceImplementation>();
        container.Register<IRepository, RepositoryImplementation>();
    }
}
container.RegisterModule(new MyModule());
```

### Scoped Resolution

```csharp
using (var scope = container.CreateScope())
{
    var scopedService = scope.Resolve<IScopedService>();
}
```

### Circular Dependency Detection

If circular dependencies exist, an exception is thrown:

```
System.Exception: Circular dependency detected: ServiceA → ServiceB → ServiceA
```

---

## Custom Injection Attributes

- Use `[Inject]` on properties for property injection.
- Use `[InjectConstructor]` to specify which constructor to use.

```csharp
public class Consumer
{
    [Inject] public IService Service { get; set; }
}
```

---

## Constructor Selection Strategy

- Prefers constructors marked with `[InjectConstructor]`.
- Otherwise, uses the constructor with the most parameters.

```csharp
public class ServiceConsumer
{
    public ServiceConsumer(IService service, IRepository repo) { }
    public ServiceConsumer(IService service) { }
}
// The constructor with most parameters is used
var consumer = container.Resolve<ServiceConsumer>();
```

---

## Testing

Unit tests ensure correctness and reliability.

### Example: Conditional Registration

```csharp
[Test]
public void ConditionalRegistration_ShouldRegister_WhenConditionIsTrue()
{
    var container = new IOCContainer(new RegistrationStore());
    bool condition = true;
    container.RegisterIf<IService, ServiceImplementation>(() => condition);

    var service = container.Resolve<IService>();

    Assert.IsNotNull(service);
}
```

### Example: Open Generics

```csharp
[Test]
public void OpenGenericRegistration_ShouldResolveCorrectly()
{
    var container = new IOCContainer(new RegistrationStore());
    container.Register(typeof(IRepository<>), typeof(Repository<>));

    var repo = container.Resolve<IRepository<User>>();

    Assert.IsNotNull(repo);
    Assert.IsInstanceOf<Repository<User>>(repo);
}
```

---

## Examples

### Registering and Resolving

```csharp
var container = new IOCContainer(new RegistrationStore());
container.Register<IService, ServiceImplementation>();
var service = container.Resolve<IService>();
```

### Using Scopes

```csharp
using (var scope = container.CreateScope())
{
    var scopedService = scope.Resolve<IService>();
}
```

---

