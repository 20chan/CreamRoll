using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using CreamRoll.Exceptions;
using CreamRoll.Helpers;
using CreamRoll.Queries;

namespace CreamRoll {
    public abstract partial class RouteServer : RouteServerBase {
        public RouteServer(string host, int port) : base(host, port) {
        }
    }

    public class RouteServer<T> : RouteServer {
        public T Instance;

        public string DefaultContentType = "text/plain; charset=utf-8";
        public Encoding DefaultContentEncoding = Encoding.UTF8;

        public RouteServer(T instance, string host = "localhost", int port = 4000, BindingFlags methodFlag = BindingFlags.Public | BindingFlags.Instance) : base(host, port) {
            Instance = instance;
            var methods = typeof(T).GetMethods(methodFlag);
            foreach (var method in methods) {
                foreach (var route in method.GetCustomAttributes<RouteAttribute>(inherit: true)) {
                    AddRoute(new Route(route.Method, route.Path, CreateRouteDelFromMethod(route, method)));
                }
            }
        }

        private AsyncRouteDel CreateRouteDelFromMethod(RouteAttribute route, MethodInfo method) {
            var returnType = method.ReturnType;
            var parameters = method.GetParameters();
            var isParamsEmpty = parameters.Length == 0;
            var isParamsCtx = parameters.Length == 1 && parameters[0].ParameterType == typeof(RouteContext);

            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>)) {
                if (route.ManuallyResponse) {
                    if (returnType.GetGenericArguments().First() == typeof(bool)) {
                        if (isParamsEmpty) {
                            return _ => method.Invoke<Task<bool>>(Instance);
                        }
                        if (isParamsCtx) {
                            return ctx => method.Invoke<Task<bool>>(Instance, ctx);
                        }
                    }
                }
                else {
                    if (isParamsEmpty) {
                        return async ctx => {
                            object body = await method.Invoke<dynamic>(Instance);
                            return await WriteDefaultResponseAsync(route, ctx.Response, body);
                        };
                    }
                    if (isParamsCtx) {
                        return async ctx => {
                            object body = await method.Invoke<dynamic>(Instance, ctx);
                            return await WriteDefaultResponseAsync(route, ctx.Response, body);
                        };
                    }
                }
            }

            if (route.ManuallyResponse) {
                if (returnType == typeof(bool)) {
                    if (isParamsEmpty) {
                        return _ => Task.FromResult(method.Invoke<bool>(Instance));
                    }
                    if (isParamsCtx) {
                        return ctx => Task.FromResult(method.Invoke<bool>(Instance, ctx));
                    }
                }
            }

            // type is object
            if (isParamsEmpty) {
                return ctx => {
                    var body = method.Invoke<object>(Instance);
                    return WriteDefaultResponseAsync(route, ctx.Response, body);
                };
            }
            if (isParamsCtx) {
                return ctx => {
                    var body = method.Invoke<object>(Instance, ctx);
                    return WriteDefaultResponseAsync(route, ctx.Response, body);
                };
            }

            throw new RouteMethodTypeMismatchException();
        }

        private async Task<bool> WriteDefaultResponseAsync(RouteAttribute route, HttpListenerResponse response, object body) {
            response.ContentType = response.ContentType ?? route.ContentType ?? DefaultContentType;
            response.ContentEncoding = response.ContentEncoding ?? route.ContentEncoding ?? DefaultContentEncoding;

            if (route.IsRedirect) {
                response.Redirect(route.RedirectLocation);
            }

            var writer = new StreamWriter(response.OutputStream);
            await writer.WriteAsync(body.ToString());
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