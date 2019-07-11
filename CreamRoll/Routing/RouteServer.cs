using System;
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

            if (returnType == typeof(Response)) {
                return req => Task.FromResult(method.Invoke<Response>(Instance, req));
            }

            if (returnType == typeof(Task<Response>)) {
                return async req => await method.Invoke<Task<Response>>(Instance, req);
            }

            throw new RouteMethodTypeMismatchException("Return type of route method must be Task<Response> or Response.");
        }

        public void Delete(string path, AsyncRouteDel action) {
            AddRoute(new Route(HttpMethod.Delete, path, action));
        }

        public void Get(string path, AsyncRouteDel action) {
            AddRoute(new Route(HttpMethod.Get, path, action));
        }

        public void Head(string path, AsyncRouteDel action) {
            AddRoute(new Route(HttpMethod.Head, path, action));
        }

        public void Options(string path, AsyncRouteDel action) {
            AddRoute(new Route(HttpMethod.Options, path, action));
        }

        public void Post(string path, AsyncRouteDel action) {
            AddRoute(new Route(HttpMethod.Post, path, action));
        }

        public void Put(string path, AsyncRouteDel action) {
            AddRoute(new Route(HttpMethod.Put, path, action));
        }

        public void Patch(string path, AsyncRouteDel action) {
            AddRoute(new Route(HttpMethod.Patch, path, action));
        }

        protected void AddRoute(Route route) {
            routes.Add(route);
        }

        protected override void ProcessRequest(HttpListenerContext ctx) {
            var request = ConvertRequestToRouteRequest(ctx.Request);
            var query = new ParameterQuery();

            Response response = null;

            foreach (var route in routes) {
                if (IsRouteMatch(route, request, ref query)) {
                    response = route.Action(request).Result;
                    break;
                }
            }

            if (response == null) {
                response = MissingRoute(request);
            }

            WriteRouteResponseToResponse(response, ctx.Response);
        }

        protected override async Task ProcessRequestAsync(HttpListenerContext ctx) {
            var request = ConvertRequestToRouteRequest(ctx.Request);
            var query = new ParameterQuery();

            Response response = null;

            foreach (var route in routes) {
                if (IsRouteMatch(route, request, ref query)) {
                    response = await route.Action(request);
                    break;
                }
            }

            if (response == null) {
                response = MissingRoute(request);
            }

            WriteRouteResponseToResponse(response, ctx.Response);
        }

        private bool IsRouteMatch(Route route, Request request, ref ParameterQuery query) {
            if (route.Method != request.Method) {
                return false;
            }

            return IsRoutePathMatch(route, request, ref query);
        }

        protected bool IsRoutePathMatch(Route route, Request request, ref ParameterQuery query) {
            var path = string.Join("/", request.Uri.Segments.Select(s => s.Replace("/", "")));
            return route.Path.TryMatch(path, ref query);
        }

        protected virtual Response MissingRoute(Request req) {
            return new Response("missing 404", status: StatusCode.NotFound);
        }

        private static Request ConvertRequestToRouteRequest(HttpListenerRequest source) {
            if (!Enum.TryParse(source.HttpMethod, true, out HttpMethod method)) {
                throw new Exception("http method parse failed");
            }
            return new Request(source.InputStream) {
                Uri = source.Url,
                Method = method,
            };
        }

        private static void WriteRouteResponseToResponse(Response source, HttpListenerResponse dest) {
            source.Contents(dest.OutputStream);
        }

        protected class Route {
            public HttpMethod Method;
            public string RawPath;
            public ParameterizedPath Path;
            public AsyncRouteDel Action;

            public Route(HttpMethod method, string path, AsyncRouteDel action) {
                Method = method;
                RawPath = path;
                Path = new ParameterizedPath(path);
                Action = action;
            }
        }
    }
}