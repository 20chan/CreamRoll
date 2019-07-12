using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static CreamRoll.Routing.Constants;

namespace CreamRoll.Routing {
    public class Response {
        public StatusCode Status = DefaultStatus;
        public HttpVersion Version = DefaultVersion;
        public HeaderMap Headers = new HeaderMap();
        public List<CreamCookie> Cookies = new List<CreamCookie>();
        public Action<Stream> Contents;

        public Response(Action<Stream> contents) {
            Contents = contents;
        }

        public Response(string contents, string contentType = null, Encoding encoding = null, StatusCode status = StatusCode.Ok) {
            encoding = encoding ?? Encoding.UTF8;
            contentType = contentType ?? "text/plain";
            contentType = contentType.Contains("charset")
                ? contentType
                : $"{contentType}; charset={encoding.WebName}";

            Headers.ContentType = contentType;
            Status = status;

            if (contents != null) {
                Contents = stream => {
                    var data = encoding.GetBytes(contents);
                    stream.Write(data, 0, data.Length);
                };
            }
        }
    }
}