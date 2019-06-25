using System.Net;
using System.Security.Principal;

namespace CreamRoll {

    public class RouteContext {
        public HttpListenerRequest Request;
        public HttpListenerResponse Response;
        public dynamic Query;
        public IPrincipal User;
    }
}