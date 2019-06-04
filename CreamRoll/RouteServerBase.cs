using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;

namespace CreamRoll {
    public abstract class RouteServerBase : Server {
        public delegate Task<bool> AsyncRouteDel(RouteContext ctx);

        protected List<Route> routes;

        public RouteServerBase() {
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

            foreach (var route in routes) {
                if (IsRouteMatch(route, request, out var routeContext)) {
                    routeContext.Request = request;
                    routeContext.Response = response;
                    routeContext.User = user;
                    if (route.Action(routeContext).Result) {
                        return;
                    }
                }
            }
        }

        protected override async Task ProcessRequestAsync(HttpListenerContext ctx) {
            var request = ctx.Request;
            var response = ctx.Response;
            var user = ctx.User;

            foreach (var route in routes) {
                if (IsRouteMatch(route, request, out var routeContext)) {
                    routeContext.Request = request;
                    routeContext.Response = response;
                    routeContext.User = user;
                    if (await route.Action(routeContext)) {
                        return;
                    }
                }
            }
        }

        private bool IsRouteMatch(Route route, HttpListenerRequest request, out RouteContext ctx) {
            if (route.Method != request.HttpMethod) {
                ctx = null;
                return false;
            }

            return IsRoutePathMatch(route.Path, request, out ctx);
        }

        protected virtual bool IsRoutePathMatch(string path, HttpListenerRequest request, out RouteContext ctx) {
            var segments = "/" + string.Join("", request.Url.Segments.Select(s => s.Replace("/", "")));
            if (path == segments) {
                ctx = new RouteContext();
                return true;
            }

            ctx = null;
            return false;
        }

        protected class Route {
            public string Method;
            public string Path;
            public AsyncRouteDel Action;

            public Route(string method, string path, AsyncRouteDel action) {
                Method = method;
                Path = path;
                Action = action;
            }
        }

        public class RouteContext {
            public HttpListenerRequest Request;
            public HttpListenerResponse Response;
            public IPrincipal User;
        }
    }
}