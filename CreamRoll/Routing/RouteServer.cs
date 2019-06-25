using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using CreamRoll.Exceptions;
using CreamRoll.Helpers;
using CreamRoll.Queries;

using Request = CreamRoll.Routing.Request<byte[]>;
using Response = CreamRoll.Routing.Response<byte[]>;

namespace CreamRoll.Routing {
    public class RouteServer<T> : Server {
        public delegate Task<Response> AsyncRouteDel(Request request);

        public T Instance;
        public string DefaultContentType = "text/plain; charset=utf-8";
        public Encoding DefaultContentEncoding = Encoding.UTF8;

        protected List<Route> routes;

        public RouteServer(T instance,
            string host = "localhost",
            int port = 4000,
            AuthenticationSchemes auth = AuthenticationSchemes.None,
            BindingFlags methodFlag = BindingFlags.Public | BindingFlags.Instance
        ) : base(host, port, auth) {
            Instance = instance;
            routes = new List<Route>();

            var methods = typeof(T).GetMethods(methodFlag);
            foreach (var method in methods) {
                foreach (var route in method.GetCustomAttributes<RouteAttribute>(inherit: true)) {
                    AddRoute(new Route(route.Method, route.Path, CreateRouteDelFromMethod(route, method)));
                }
            }
        }

        private AsyncRouteDel CreateRouteDelFromMethod(RouteAttribute route, MethodInfo method) {
            var returnType = method.ReturnType;
            var parameterInfos = method.GetParameters();
            var isParamsEmpty = parameterInfos.Length == 0;

            if (returnType == typeof(Task<Response>)) {
                return async req => await method.Invoke<Task<Response>>(Instance, GetParams(isParamsEmpty));
            }

            throw new RouteMethodTypeMismatchException("Return type of route method must be Task<Response> or Response.");
        }

        private static object[] GetParams(bool isEmpty) {
            if (isEmpty) {
                return new object[0];
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

        public void Delete(string path, AsyncRouteDel action) {
            AddRoute(new Route("DELETE", path, action));
        }

        public void Get(string path, AsyncRouteDel action) {
            AddRoute(new Route("GET", path, action));
        }

        public void Head(string path, AsyncRouteDel action) {
            AddRoute(new Route("HEAD", path, action));
        }

        public void Options(string path, AsyncRouteDel action) {
            AddRoute(new Route("OPTIONS", path, action));
        }

        public void Post(string path, AsyncRouteDel action) {
            AddRoute(new Route("POST", path, action));
        }

        public void Put(string path, AsyncRouteDel action) {
            AddRoute(new Route("PUT", path, action));
        }

        public void Patch(string path, AsyncRouteDel action) {
            AddRoute(new Route("PATCH", path, action));
        }

        protected void AddRoute(Route route) {
            routes.Add(route);
        }

        protected override void ProcessRequest(HttpListenerContext ctx) {
            var request = ctx.Request;
            var response = ctx.Response;
            var user = ctx.User;
            var query = new ParameterQuery();

            foreach (var route in routes) {
                if (IsRouteMatch(route, request, ref query)) {
                    if (route.Action(routeContext).Result) {
                        return;
                    }
                }
            }

            HandleMissingRoute(routeContext);
        }

        protected override async Task ProcessRequestAsync(HttpListenerContext ctx) {
            var request = ctx.Request;
            var response = ctx.Response;
            var user = ctx.User;
            var query = new ParameterQuery();
            var routeContext = new RouteContext {
                Request = request,
                Response = response,
                User = user,
                Query = query,
            };

            foreach (var route in routes) {
                if (IsRouteMatch(route, request, ref query)) {
                    if (await route.Action(routeContext)) {
                        return;
                    }
                }
            }

            await HandleMissingRouteAsync(routeContext);
        }

        private bool IsRouteMatch(Route route, HttpListenerRequest request, ref ParameterQuery query) {
            if (route.Method != request.HttpMethod) {
                return false;
            }

            return IsRoutePathMatch(route, request, ref query);
        }

        protected bool IsRoutePathMatch(Route route, HttpListenerRequest request, ref ParameterQuery query) {
            var path = string.Join("/", request.Url.Segments.Select(s => s.Replace("/", "")));
            return route.Path.TryMatch(path, ref query);
        }

        protected virtual Response MissingRoute(Request req) {
            return new Response();
            ctx.Response.StatusCode = 404;
            ctx.Response.OutputStream.Close();
        }

        protected virtual Task<Response> MissingRouteAsync(Request req) {
            ctx.Response.StatusCode = 404;
            ctx.Response.OutputStream.Close();
            return Task.CompletedTask;
        }

        protected class Route {
            public string Method;
            public string RawPath;
            public ParameterizedPath Path;
            public AsyncRouteDel Action;

            public Route(string method, string path, AsyncRouteDel action) {
                Method = method;
                RawPath = path;
                Path = new ParameterizedPath(path);
                Action = action;
            }
        }
    }
}