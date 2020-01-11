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

        public string BaseUri;
        public string AbsolutePathWithoutBaseUri;

        public ParameterQuery Query;
        public Stream Body;

        public Request(Stream body) {
            Body = body;
        }

        public void SetBaseUri(string baseUri) {
            var fullPath = Uri.AbsolutePath;
            if (string.IsNullOrEmpty(baseUri)) {
                BaseUri = null;
                AbsolutePathWithoutBaseUri = fullPath;
                return;
            }
            BaseUri = baseUri;
            var unwrapedBase = baseUri.TrimStart('/').TrimEnd('/');
            AbsolutePathWithoutBaseUri = fullPath.TrimStart('/').Substring(unwrapedBase.Length);
        }
    }
}