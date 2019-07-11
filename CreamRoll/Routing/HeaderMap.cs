using System.Collections.Generic;

namespace CreamRoll.Routing {
    public class HeaderMap : Dictionary<string, string> {
        public string ContentType {
            get => base["content-type"];
            set => base["content-type"] = value;
        }
    }
}