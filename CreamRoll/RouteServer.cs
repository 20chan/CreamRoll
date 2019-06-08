using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using CreamRoll.Exceptions;
using CreamRoll.Helpers;
using CreamRoll.Queries;

namespace CreamRoll {
    public abstract partial class RouteServer : RouteServerBase {
    }

    public class RouteServer<T> : RouteServer {
        public T Instance;

        public RouteServer(T instance, BindingFlags methodFlag = BindingFlags.Public | BindingFlags.Instance) {
            Instance = instance;
            var methods = typeof(T).GetMethods(methodFlag);
            foreach (var method in methods) {
                foreach (var route in method.GetCustomAttributes<RouteAttribute>(inherit: true)) {
                    AddRoute(new Route(route.Method, route.Path, CreateRouteDelFromMethod(method)));
                }
            }
        }

        private AsyncRouteDel CreateRouteDelFromMethod(MethodInfo method) {
            var returnType = method.ReturnType;
            var parameters = method.GetParameters();
            var isParamsEmpty = parameters.Length == 0;
            var isParamsCtx = parameters.Length == 1 && parameters[0].ParameterType == typeof(RouteContext);

            if (returnType == typeof(Task<bool>)) {
                if (isParamsEmpty) {
                    return _ => method.Invoke<Task<bool>>(Instance);
                }
                if (isParamsCtx) {
                    return ctx => method.Invoke<Task<bool>>(Instance, ctx);
                }

                throw new RouteMethodTypeMismatchException();
            }

            if (returnType == typeof(Task<string>)) {
                if (isParamsEmpty) {
                    return ctx => {
                        var body = method.Invoke<Task<string>>(Instance);
                        return WriteDefaultResponseAsync(ctx.Response, body);
                    };
                }
                if (isParamsCtx) {
                    return ctx => {
                        var body = method.Invoke<Task<string>>(Instance, ctx);
                        return WriteDefaultResponseAsync(ctx.Response, body);
                    };
                }
            }

            if (returnType == typeof(bool)) {
                if (isParamsEmpty) {
                    return _ => Task.FromResult(method.Invoke<bool>(Instance));
                }
                if (isParamsCtx) {
                    return ctx => Task.FromResult(method.Invoke<bool>(Instance, ctx));
                }
            }

            if (returnType == typeof(string)) {
                if (isParamsEmpty) {
                    return ctx => {
                        var body = method.Invoke<string>(Instance);
                        return WriteDefaultResponseAsync(ctx.Response, body);
                    };
                }
                if (isParamsCtx) {
                    return ctx => {
                        var body = method.Invoke<string>(Instance, ctx);
                        return WriteDefaultResponseAsync(ctx.Response, body);
                    };
                }
            }

            throw new RouteMethodTypeMismatchException();
        }

        private static async Task<bool> WriteDefaultResponseAsync(HttpListenerResponse response, Task<string> body) {
            return await WriteDefaultResponseAsync(response, await body);
        }

        private static async Task<bool> WriteDefaultResponseAsync(HttpListenerResponse response, string body) {
            if (response.ContentType != null) {
                response.ContentType = "text/html; charset=utf-8";
            }
            response.ContentEncoding = Encoding.UTF8;
            var writer = new StreamWriter(response.OutputStream);
            await writer.WriteAsync(body);
            await writer.FlushAsync();
            writer.Close();
            return true;
        }

        protected override bool IsRoutePathMatch(Route route, HttpListenerRequest request, ref ParameterQuery query) {
            var path = string.Join("/", request.Url.Segments.Select(s => s.Replace("/", "")));
            return route.Path.TryMatch(path, ref query);
        }
    }
}