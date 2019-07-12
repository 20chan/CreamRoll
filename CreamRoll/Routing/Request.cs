using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using CreamRoll.Queries;
using static CreamRoll.Routing.Constants;

namespace CreamRoll.Routing {
    public class Request {
        public HttpMethod Method = DefaultMethod;
        public Uri Uri;
        public HttpVersion Version = DefaultVersion;
        public HeaderMap Headers = new HeaderMap();
        public Dictionary<string, CreamCookie> Cookies = new Dictionary<string, CreamCookie>();
        public IPrincipal User;

        public ParameterQuery Query;
        public Stream Body;

        public Request(Stream body) {
            Body = body;
        }
    }
}