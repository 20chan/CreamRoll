using System.Collections.Specialized;

namespace CreamRoll.Queries {
    public class ParameterizedPath {
        private readonly string path;
        private readonly IPathSegment[] segments;
        private readonly QuerySegment[] queries;

        public ParameterizedPath(string path) {
            this.path = path;
            SplitPath(path, out var segment, out var query);
            segments = ParseSegments(segment);
            queries = ParseQueries(query);
        }

        public bool TryMatch(string urlPath, string queryString, ref ParameterQuery query) {
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

            var queryCollection = System.Web.HttpUtility.ParseQueryString(queryString);
            foreach (var q in queries) {
                q.Match(queryCollection, ref query);
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

        private static QuerySegment[] ParseQueries(string path) {
            if (string.IsNullOrEmpty(path)) {
                return new QuerySegment[0];
            }
            var split = path.Split('&');
            var result = new QuerySegment[split.Length];
            for (var i = 0; i < split.Length; i++) {
                result[i] = new QuerySegment(split[i]);
            }

            return result;
        }

        private static void SplitPath(string fullPath, out string path, out string query) {
            var pathSplit = fullPath.Split('?');
            path = pathSplit[0];
            query = pathSplit.Length < 2 ? "" : pathSplit[1].Split('#')[0];
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
            private readonly bool isInteger;

            public NamedPathSegment(string part) {
                var inner = part.Trim('{', '}');
                var split = inner.Split(':');
                name = split[0];
                if (split.Length == 0) {
                    name = split[0];
                    isInteger = false;
                }
                if (split.Length == 1) {
                    isInteger = true;
                }
            }

            public bool DoesMatch(string part) {
                return isInteger && int.TryParse(part, out _);
            }

            public void Match(string part, ref ParameterQuery query) {
                query[name] = part;
            }
        }

        private class QuerySegment {
            private readonly string name;
            private readonly string defaultValue;

            public QuerySegment(string query) {
                var split = query.Split('=');
                name = split[0].Trim('{', '}');
                defaultValue = split.Length < 2 ? null : split[1];
            }

            public QuerySegment(string name, string defaultValue) {
                this.name = name;
                this.defaultValue = defaultValue;
            }

            public void Match(NameValueCollection queryString, ref ParameterQuery query) {
                query[name] = queryString[name] ?? defaultValue;
            }
        }
    }
}