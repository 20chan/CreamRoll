using System;
using System.Collections.Generic;
using System.Security.Principal;
using CreamRoll.Queries;
using static CreamRoll.Routing.Constants;

namespace CreamRoll.Routing {
    public class Request<T> {
        public RequestHead Head;
        public ParameterQuery Query;
        public T Body;

        public Request(Uri uri, T body) {
            Head = new RequestHead(uri);
            Body = body;
        }
    }

    public class RequestHead {
        public HttpMethod Method = DefaultMethod;
        public Uri Uri;
        public HttpVersion Version = DefaultVersion;
        public HeaderMap Headers = new HeaderMap();
        public IPrincipal User;

        public RequestHead(Uri uri) {
            Uri = uri;
        }
    }
}