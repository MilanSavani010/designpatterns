using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.IOC;

public class LoggingInterceptor : Interceptor
{
    public override object Invoke(MethodInfo method, object[] args, Func<object> proceed)
    {
        Console.WriteLine($"[LOG] Calling {method.Name} with arguments: {string.Join(",",args)}");
        var result = proceed();
        Console.WriteLine($"[LOG] {method.Name} returned {result}");
        return result;
    }
}
