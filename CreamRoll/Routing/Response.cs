using System.Collections.Generic;
using static CreamRoll.Routing.Constants;

namespace CreamRoll.Routing {
    public class Response<T> {
        public ResponseHead Head;
        public T Body;

        public Response(T body) {
            Head = new ResponseHead();
            Body = body;
        }
    }

    public class ResponseHead {
        public StatusCode Status = DefaultStatus;
        public HttpVersion Version = DefaultVersion;
        public HeaderMap Headers = new HeaderMap();

        public ResponseHead() {

        }
    }
}