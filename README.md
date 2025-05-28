# designpatterns

## Overview

This repository contains a lightweight, extensible Inversion of Control (IoC) container implemented in C#. The container provides dependency injection, lifetime management, interception, and advanced registration features for building modular and testable applications.

## Features

- **Dependency Registration & Resolution**: Register and resolve interfaces and implementations.
- **Lifetimes**: Supports Transient, Singleton, and Scoped lifetimes.
- **Constructor & Property Injection**: Automatically injects dependencies via constructors or `[Inject]`-decorated properties.
- **Open Generics**: Register and resolve open generic types.
- **Named Registrations**: Register multiple implementations for the same interface using names.
- **Conditional Registration**: Register services based on runtime conditions.
- **Module Support**: Group registrations in modules.
- **Automatic Assembly Scanning**: Register all types in an assembly.
- **Interceptors**: Add cross-cutting logic (e.g., logging, timing) via dynamic proxies.
- **Circular Dependency Detection**: Prevents infinite loops in dependency graphs.
- **Thread Safety**: Safe for concurrent resolutions.

## Getting Started

### Registering Services

```csharp
var container = new IOCContainer(new RegistrationStore());
container.Register<IService, ServiceImplementation>();
```

### Resolving Services

```csharp
var service = container.Resolve<IService>();
```

### Using Interceptors

```csharp
container.RegisterInterceptor<IService>(new LoggingInterceptor());
container.RegisterInterceptor<IService>(new TimingInterceptor());
var service = container.Resolve<IService>();
```

### Scoped Lifetimes

```csharp
using (var scope = container.CreateScope())
{
    var scopedService = scope.Resolve<IService>();
}
```

### Open Generics

```csharp
container.Register(typeof(IRepository<>), typeof(Repository<>));
var userRepo = container.Resolve<IRepository<User>>();
```

## Project Structure

```
IOC/
  code/
    Interceptor.cs
    InterceptorProxy.cs
    IOCContainer.cs
    Registration.cs
    InterceptorImplementation/
      LoggingInterceptor.cs
      TimingInterceptor.cs
  docs/
    Features.md
  test/
    SimpleContainerTests.cs
README.md
```

- **code/**: Core IoC container implementation and interceptors.
- **docs/**: Documentation and feature guide.
- **test/**: Unit tests for the container.

## Documentation

See [IOC/docs/Features.md](IOC/docs/Features.md) for a detailed user guide and API documentation.

## Testing

Unit tests are provided in [IOC/test/SimpleContainerTests.cs](IOC/test/SimpleContainerTests.cs) and cover all major features.

---

© 2024 Your Name or Organization. Licensed under MIT.