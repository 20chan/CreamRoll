using System;

namespace CreamRoll {
    public abstract partial class RouteServer {
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        public class RouteAttribute : Attribute {
            public string Method;
            public string Path;

            public RouteAttribute(string method, string path) {
                Method = method;
                Path = path;
            }
        }

        public class DeleteAttribute : RouteAttribute {
            public DeleteAttribute(string path) : base("DELETE", path) {
            }
        }

        public class GetAttribute : RouteAttribute {
            public GetAttribute(string path) : base("GET", path) {
            }
        }

        public class HeadAttribute : RouteAttribute {
            public HeadAttribute(string path) : base("HEAD", path) {
            }
        }

        public class OptionsAttribute : RouteAttribute {
            public OptionsAttribute(string path) : base("OPTIONS", path) {
            }
        }

        public class PostAttribute : RouteAttribute {
            public PostAttribute(string path) : base("POST", path) {
            }
        }

        public class PutAttribute : RouteAttribute {
            public PutAttribute(string path) : base("PUT", path) {
            }
        }

        public class PatchAttribute : RouteAttribute {
            public PatchAttribute(string path) : base("PATCH", path) {
            }
        }
    }
}