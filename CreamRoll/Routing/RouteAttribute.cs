using System;
using System.Text;

namespace CreamRoll.Routing {
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RouteAttribute : Attribute {
        public HttpMethod Method;
        public string Path;

        public string ContentType;
        public Encoding ContentEncoding;

        public RouteAttribute(HttpMethod method, string path) {
            Method = method;
            Path = path;
        }
    }

    public class DeleteAttribute : RouteAttribute {
        public DeleteAttribute(string path) : base(HttpMethod.Delete, path) {
        }
    }

    public class GetAttribute : RouteAttribute {
        public GetAttribute(string path) : base(HttpMethod.Get, path) {
        }
    }

    public class HeadAttribute : RouteAttribute {
        public HeadAttribute(string path) : base(HttpMethod.Head, path) {
        }
    }

    public class OptionsAttribute : RouteAttribute {
        public OptionsAttribute(string path) : base(HttpMethod.Options, path) {
        }
    }

    public class PostAttribute : RouteAttribute {
        public PostAttribute(string path) : base(HttpMethod.Post, path) {
        }
    }

    public class PutAttribute : RouteAttribute {
        public PutAttribute(string path) : base(HttpMethod.Put, path) {
        }
    }

    public class PatchAttribute : RouteAttribute {
        public PatchAttribute(string path) : base(HttpMethod.Patch, path) {
        }
    }
}