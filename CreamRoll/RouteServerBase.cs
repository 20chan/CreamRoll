using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using CreamRoll.Queries;

namespace CreamRoll {
    public abstract class RouteServerBase : Server {
        public delegate Task<bool> AsyncRouteDel(RouteContext ctx);

        protected List<Route> routes;

        public RouteServerBase(string host, int port) : base(host, port) {
            routes = new List<Route>();
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
            var routeContext = new RouteContext {
                Request = request,
                Response = response,
                User = user,
                Query = query,
            };

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

        protected virtual bool IsRoutePathMatch(Route route, HttpListenerRequest request, ref ParameterQuery query) {
            var segments = "/" + string.Join("/", request.Url.Segments.Select(s => s.Replace("/", "")));
            return route.RawPath == segments;
        }

        protected virtual void HandleMissingRoute(RouteContext ctx) {
            ctx.Response.StatusCode = 404;
            ctx.Response.OutputStream.Close();
        }

        protected virtual Task HandleMissingRouteAsync(RouteContext ctx) {
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

        public class RouteContext {
            public HttpListenerRequest Request;
            public HttpListenerResponse Response;
            public dynamic Query;
            public IPrincipal User;
        }
    }
}