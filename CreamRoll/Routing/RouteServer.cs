using System;
using System.Collections.Generic;
using System.Linq;
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

        protected List<Route> routes;

        public RouteServer(T instance,
            string host = "localhost",
            int port = 4000,
            AuthenticationSchemes auth = AuthenticationSchemes.None,
            BindingFlags methodFlag = BindingFlags.Public | BindingFlags.Instance
        ) : base(host, port, auth) {
            Instance = instance;
            routes = new List<Route>();

            AppendRoutes<T>(instance, methodFlag);
        }

        public void AppendRoutes<TClass>(TClass instance, BindingFlags methodFlag = BindingFlags.Public | BindingFlags.Instance) {
            var methods = typeof(TClass).GetMethods(methodFlag);
            foreach (var method in methods) {
                foreach (var route in method.GetCustomAttributes<RouteAttribute>(inherit: true)) {
                    AddRoute(new Route(route.Method, route.Path, CreateRouteDelFromMethod(route, method, instance)));
                }
            }
        }

        private AsyncRouteDel CreateRouteDelFromMethod<TClass>(RouteAttribute route, MethodInfo method, TClass instance) {
            var returnType = method.ReturnType;
            var parameterInfos = method.GetParameters();
            var isParamsEmpty = parameterInfos.Length == 0;

            if (returnType == typeof(Response)) {
                return req => Task.FromResult(method.Invoke<Response>(instance, req));
            }

            if (returnType == typeof(Task<Response>)) {
                return async req => await method.Invoke<Task<Response>>(instance, req);
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

        protected override async Task ProcessRequestAsync(HttpListenerContext ctx) {
            var request = ConvertRequestToRouteRequest(ctx);
            var query = new ParameterQuery();

            Response response = null;

            foreach (var route in routes) {
                if (IsRouteMatch(route, request, ref query)) {
                    request.Query = query;
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
            return route.Path.TryMatch(path, request.Uri.Query, ref query);
        }

        protected virtual Response MissingRoute(Request req) {
            return new TextResponse("missing 404", status: StatusCode.NotFound);
        }

        private static Request ConvertRequestToRouteRequest(HttpListenerContext ctx) {
            var source = ctx.Request;
            if (!Enum.TryParse(source.HttpMethod, true, out HttpMethod method)) {
                throw new Exception("http method parse failed");
            }

            var res = new Request(source.InputStream) {
                Uri = source.Url,
                Method = method,
                User = ctx.User,
            };
            foreach (var key in source.Headers.AllKeys) {
                res.Headers[key] = source.Headers[key];
            }
            foreach (Cookie cookie in source.Cookies) {
                res.Cookies.Add(cookie.Name, new CreamCookie(cookie));
            }

            return res;
        }

        private static void WriteRouteResponseToResponse(Response source, HttpListenerResponse dest) {
            foreach (var header in source.Headers) {
                dest.Headers[header.Key] = header.Value;
            }
            dest.StatusCode = (int)source.Status;
            foreach (var cookie in source.Cookies) {
                dest.Cookies.Add(new Cookie(cookie.Name, cookie.Value));
            }

            source.WriteContent(dest.OutputStream);
            dest.OutputStream.Close();
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