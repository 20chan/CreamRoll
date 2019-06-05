namespace CreamRoll.Queries {
    public class ParameterizedPath {
        private readonly string path;
        private readonly IPathSegment[] segments;

        public ParameterizedPath(string path) {
            this.path = path;
            segments = ParseSegments(path);
        }

        public bool TryMatch(string urlPath, ref ParameterQuery query) {
            var urlSegments = urlPath.Trim('/').Split('/');

            if (segments.Length != urlSegments.Length) {
                return false;
            }

            for (int i = 0; i < segments.Length; i++) {
                if (!segments[i].DoesMatch(urlSegments[i])) {
                    return false;
                }
            }

            for (int i = 0; i < segments.Length; i++) {
                segments[i].Match(urlSegments[i], ref query);
            }

            return true;
        }

        private static IPathSegment[] ParseSegments(string path) {
            path = path.Trim('/');
            var split = path.Split('/');
            var result = new IPathSegment[split.Length];

            for (int i = 0; i < split.Length; i++) {
                if (split[i].StartsWith("{")) {
                    result[i] = new NamedPathSegment(split[i]);
                }
                else {
                    result[i] = new ConstPathSegment(split[i]);
                }
            }

            return result;
        }

        private interface IPathSegment {
            bool DoesMatch(string part);
            void Match(string part, ref ParameterQuery query);
        }

        private class ConstPathSegment : IPathSegment {
            private readonly string name;

            public ConstPathSegment(string part) {
                name = part;
            }

            public bool DoesMatch(string part) {
                return name == part;
            }

            public void Match(string part, ref ParameterQuery query) {

            }
        }

        private class NamedPathSegment : IPathSegment {
            private readonly string name;

            public NamedPathSegment(string part) {
                name = part.Trim('{', '}');
            }

            public bool DoesMatch(string part) {
                return true;
            }

            public void Match(string part, ref ParameterQuery query) {
                query[name] = part;
            }
        }
    }
}