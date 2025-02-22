# IOC Container Documentation and User Guide

## Introduction
The IOC Container is a lightweight dependency injection framework that provides powerful features to manage object creation, lifetimes, and dependency resolution in a flexible manner. This guide covers the features, usage, and implementation details of the IOC Container.

## Features Overview
The IOC Container supports the following features:

1. **Basic Registration and Resolution**
   - Register and resolve interfaces and their implementations.
   
2. **Constructor Injection**
   - Supports automatic constructor selection based on parameter count.
   - Ignores primitive types during dependency resolution.

3. **Property Injection**
   - Inject dependencies into properties using the `[Inject]` attribute.

4. **Singleton, Scoped, and Transient Lifetimes**
   - Singleton: One instance per container.
   - Scoped: One instance per scope.
   - Transient: New instance each time resolved.

5. **Scoped Container Support**
   - Create sub-containers to handle scoped lifetimes.

6. **Open Generics Support**
   - Register and resolve open generic types dynamically.

7. **Automatic Module Registration**
   - Register all types from an assembly automatically.

8. **Conditional Registration**
   - Register implementations conditionally based on runtime conditions.

9. **Circular Dependency Detection**
   - Prevents infinite loops by detecting circular dependencies.

## Installation and Setup
To use the IOC Container, ensure you have the required class definitions implemented as per the provided codebase.

## Usage

### Registering Dependencies
```csharp
var container = new IOCContainer(new RegistrationStore());
container.Register<IService, ServiceImplementation>();
```

### Resolving Dependencies
```csharp
var service = container.Resolve<IService>();
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

### Scoped Lifetimes
```csharp
using (var scope = container.CreateScope())
{
    var scopedService = scope.Resolve<IService>();
}
```

### Open Generic Type Support
```csharp
container.Register(typeof(IRepository<>), typeof(Repository<>));
var userRepository = container.Resolve<IRepository<User>>();
```

### Conditional Registration
```csharp
container.RegisterIf<IService, SpecialService>(() => DateTime.Now.Hour < 12);
```

---
---
---

### **IoC Container: Documentation and User Guide**  

#### **Introduction**  
This IoC (Inversion of Control) container is a lightweight and feature-rich dependency injection framework that provides extensive capabilities for managing dependencies in a structured and scalable manner. It supports multiple registration and resolution strategies, ensuring high flexibility and maintainability in software development.  

This document serves as a comprehensive guide for understanding and utilizing the IoC container, covering its core functionalities and advanced features.  

---

## **1. Features Overview**  

The IoC container currently supports the following features:  

### **Core Features:**  
1. **Basic Dependency Injection** – Supports registering and resolving interfaces and their implementations.  
2. **Singleton, Transient, and Scoped Lifetimes** – Allows precise control over object lifetimes.  
3. **Constructor Injection** – Automatically resolves dependencies based on constructor parameters.  
4. **Property Injection** – Enables injecting dependencies into class properties.  
5. **Named Registrations** – Supports multiple implementations for the same interface, resolved using a name identifier.  

### **Advanced Features:**  
6. **Automatic Registration (Assembly Scanning)** – Registers all types implementing an interface within an assembly.  
7. **Open Generic Type Support** – Allows registering generic interfaces and resolving closed generic types dynamically.  
8. **Conditional Registration** – Registers dependencies based on runtime conditions.  
9. **Module-Based Registration** – Enables encapsulating multiple registrations in modules for better organization.  
10. **Lazy Dependency Resolution** – Supports resolving dependencies on-demand without creating instances prematurely.  
11. **Multi-Thread Safety** – Ensures safe usage in concurrent environments using synchronization mechanisms.  
12. **Constructor Selection Strategy** – Automatically selects the most suitable constructor for dependency injection.  
13. **Circular Dependency Detection** – Prevents infinite loops caused by circular references in dependencies.  
14. **Scoped Resolution** – Ensures dependencies are resolved within a controlled lifetime scope.  
15. **Factory-Based Registration** – Supports registering dependencies using factory functions.  
16. **Primitive Type Handling in Constructor Injection** – Ignores primitive types when resolving constructor dependencies.  
17. **Custom Attribute Support for Injection** – Uses `[Inject]` and `[InjectConstructor]` attributes to mark properties and constructors for dependency injection.  
18. **Extensibility & Customization** – Allows modifying and extending the container to fit specific project needs.  

---

## **2. Getting Started**  

### **2.1 Installation**  
To integrate this IoC container into your project, add the required source files and reference the namespace:  

```csharp
using ProductService.IOC;
```

---

## **3. Registering Dependencies**  

### **3.1 Basic Registration**  
You can register interfaces and their implementations as follows:  

```csharp
var container = new IOCContainer(new RegistrationStore());
container.Register<IService, ServiceImplementation>();
```

By default, dependencies are registered with a **transient lifetime**, meaning a new instance is created each time it is resolved.  

### **3.2 Specifying Lifetime**  
You can specify the lifetime when registering a dependency:  

```csharp
container.Register<IService, ServiceImplementation>(lifetime: Lifetime.Singleton);
```
- **Transient:** New instance is created on each resolution.  
- **Singleton:** Same instance is used throughout the application.  
- **Scoped:** Same instance is used within a scope.  

### **3.3 Factory-Based Registration**  
You can register dependencies using a factory function:  

```csharp
container.Register<IService>(c => new ServiceImplementation(), lifetime: Lifetime.Singleton);
```

---

## **4. Resolving Dependencies**  

### **4.1 Resolving a Type**  
```csharp
var service = container.Resolve<IService>();
```

### **4.2 Resolving Named Instances**  
```csharp
container.Register<IService, ServiceA>("ServiceA");
container.Register<IService, ServiceB>("ServiceB");

var serviceA = container.Resolve<IService>("ServiceA");
var serviceB = container.Resolve<IService>("ServiceB");
```

### **4.3 Handling Constructor Injection**  
If a class has a constructor with dependencies, the container automatically resolves them:  

```csharp
public class ServiceConsumer
{
    private readonly IService _service;
    
    public ServiceConsumer(IService service)
    {
        _service = service;
    }
}

var consumer = container.Resolve<ServiceConsumer>();
```

---

## **5. Advanced Features**  

### **5.1 Open Generic Type Support**  
You can register generic interfaces and their implementations:  

```csharp
container.Register(typeof(IRepository<>), typeof(Repository<>));
```
Then resolve a closed generic type:  
```csharp
var userRepository = container.Resolve<IRepository<User>>();
```

### **5.2 Conditional Registration**  
Allows registering a dependency only if a condition is met:  

```csharp
container.RegisterIf<IService, ProductionService>(() => Environment.GetEnvironmentVariable("ENV") == "PROD");
```

### **5.3 Module-Based Registration**  
Encapsulates multiple registrations into a module:  

```csharp
public class MyModule : IModule
{
    public void Register(IOCContainer container)
    {
        container.Register<IService, ServiceImplementation>();
        container.Register<IRepository, RepositoryImplementation>();
    }
}

// Register module
container.RegisterModule(new MyModule());
```

### **5.4 Scoped Resolution**  
Creates a scope to manage scoped dependencies:  

```csharp
using (var scope = container.CreateScope())
{
    var scopedService = scope.Resolve<IScopedService>();
}
```

### **5.5 Circular Dependency Detection**  
If circular dependencies exist, an exception is thrown:  

```plaintext
System.Exception: Circular dependency detected: ServiceA → ServiceB → ServiceA
```

---

## **6. Custom Injection Attributes**  

### **6.1 Property Injection**  
Mark properties with `[Inject]` to have dependencies injected automatically:  

```csharp
public class Consumer
{
    [Inject] public IService Service { get; set; }
}
```

---

## **7. Constructor Selection Strategy**  

The container selects the most suitable constructor based on:  
- The presence of `[InjectConstructor]` attribute.  
- The constructor with the most parameters (if no attributes are used).  

Example:  

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

## **8. Testing**  

This IoC container has an extensive test suite using **TestFixture** to ensure correctness and reliability.  

### **8.1 Testing Conditional Registration**  
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

### **8.2 Testing Open Generics**  
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
---
---
