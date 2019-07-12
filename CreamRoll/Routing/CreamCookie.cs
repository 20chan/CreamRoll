using System;
using System.Net;

namespace CreamRoll.Routing {
    public class CreamCookie {
        public string Name;
        public string Value;
        public bool Secure;
        public DateTime? Expires;

        public CreamCookie(string name, string value) {
            Name = name;
            Value = value;
        }

        public CreamCookie(Cookie cookie) {
            Name = cookie.Name;
            Value = cookie.Value;
            Secure = cookie.Secure;
            Expires = cookie.Expired ? null : (DateTime?)cookie.Expires;
        }
    }
}