using System.Reflection;

namespace ProductService.IOC;

public interface IProxyMaker
{

}
public class InterceptorProxy : DispatchProxy,IProxyMaker
{
    private object _target;
    private List<Interceptor> _interceptors;

    public static object Create(Type serviceType,object target, params Interceptor[] interceptors)
    {
        var proxy = (InterceptorProxy)DispatchProxy.Create(serviceType, typeof(InterceptorProxy));
        proxy._target = target;
        proxy._interceptors = interceptors.ToList();
        return proxy;
    }

    protected override object Invoke(MethodInfo targetMethod, object[] args)
    {
        // Create a pipeline to execute interceptors in order
        int index = -1;
        
        return Handler();


        //this function is nested local function.
        //it processes interceptors sequentially,one by one using recursion.
        //if all interceptors have been executed, it invokes the actual target method.
        object Handler()
        {
            index++;
            return index < _interceptors.Count
                ? _interceptors[index].Invoke(targetMethod, args, Handler)
                : (_target != null ? targetMethod.Invoke(_target, args) : throw new InvalidOperationException("Target is null"));
        }
    }
}
