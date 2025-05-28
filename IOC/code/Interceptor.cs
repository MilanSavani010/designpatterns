using System.Reflection;

namespace ProductService.IOC;

public abstract class Interceptor
{
    public abstract object Invoke(MethodInfo method, object[] args,Func<object> proceed);
}
