using System;

namespace CreamRoll.Exceptions {
    public class RouteMethodTypeMismatchException : Exception {
        public RouteMethodTypeMismatchException() {}
        public RouteMethodTypeMismatchException(string msg) : base(msg) {}
    }
}