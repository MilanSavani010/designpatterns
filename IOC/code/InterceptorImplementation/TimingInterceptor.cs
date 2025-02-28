using System.Reflection;

namespace ProductService.IOC;

public class TimingInterceptor : Interceptor
{
    public override object Invoke(MethodInfo method, object[] args, Func<object> proceed)
    {
        var start = DateTime.Now;
        var result = proceed();
        var end = DateTime.Now;
        Console.WriteLine($"[TIMER] {method.Name} took {(end - start).TotalMilliseconds} ms");
        return result;
    }
}
