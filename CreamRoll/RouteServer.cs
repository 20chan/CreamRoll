using System;
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
            var isReturnNonGenericTask = returnType == typeof(Task);
            var isReturnGenericTask = returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>);
            var isAsync = isReturnNonGenericTask || isReturnGenericTask;
            var parameterInfos = method.GetParameters();
            var isParamsEmpty = parameterInfos.Length == 0;
            var isParamsCtx = parameterInfos.Length == 1 && parameterInfos[0].ParameterType == typeof(RouteContext);

            if (isAsync) {
                if (route.ManuallyResponse) {
                    if (isReturnNonGenericTask) {
                        return ctx => method.Invoke<Task>(Instance, getParams(ctx))
                            .ContinueWith(_ => true);
                    }

                    if (isReturnGenericTask) {
                        if (returnType.GetGenericArguments().First() == typeof(bool)) {
                            return ctx => method.Invoke<Task<bool>>(Instance, getParams(ctx));
                        }
                    }

                    throw new RouteMethodTypeMismatchException("Return type of async ManuallyResponse method must be Task<bool> or Task");
                }
                if (isReturnNonGenericTask) {
                    // Not-ManuallyResponse route method should not return nothing... but...
                    return async ctx => {
                        await method.Invoke<Task>(Instance, getParams(ctx));
                        return await WriteDefaultResponseAsync(route, ctx.Response, "");
                    };
                }
                if (isReturnGenericTask) {
                    return async ctx => {
                        object body = await method.Invoke<dynamic>(Instance, getParams(ctx));
                        return await WriteDefaultResponseAsync(route, ctx.Response, body);
                    };
                }
            }

            if (route.ManuallyResponse) {
                if (returnType == typeof(void)) {
                    return ctx => Task.FromResult(method.InvokeAndTrue(Instance, getParams(ctx)));
                }
                if (returnType == typeof(bool)) {
                    return ctx => Task.FromResult(method.Invoke<bool>(Instance, getParams(ctx)));
                }

                throw new RouteMethodTypeMismatchException("Return type of ManuallyResponse method must be bool or void");
            }

            if (returnType == typeof(void)) {
                return ctx => {
                    method.InvokeAndTrue(Instance, getParams(ctx));
                    return WriteDefaultResponseAsync(route, ctx.Response, "");
                };
            }

            return ctx => {
                var body = method.Invoke<object>(Instance, getParams(ctx));
                return WriteDefaultResponseAsync(route, ctx.Response, body);
            };


            object[] getParams(RouteContext ctx) {
                if (isParamsEmpty) {
                    return new object[0];
                }
                if (isParamsCtx) {
                    return new object[] { ctx };
                }

                throw new RouteMethodTypeMismatchException("Parameters of route method must be () or (RouteContext)");
            }
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