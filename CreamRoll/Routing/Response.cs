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

        public virtual void WriteContent(Stream body) {
            Contents(body);
        }

        public Response() {
            Contents = null;
        }

        public Response(Action<Stream> contents) {
            Contents = contents;
        }
    }

    public class TextResponse : Response {
        public string ContentText;
        public Encoding Encoding;

        private string _contentType;
        public string ContentType {
            get => _contentType;
            set {
                _contentType = value ?? "text/plain";
                _contentType = _contentType.Contains("charset")
                    ? _contentType
                    : $"{_contentType}; charset={Encoding.WebName}";
                Headers.ContentType = _contentType;
            }
        }

        public TextResponse(string contents, string contentType = null, Encoding encoding = null, StatusCode status = StatusCode.Ok) {
            Encoding = encoding ?? Encoding.UTF8;
            ContentType = contentType;
            Status = status;
            ContentText = contents;
        }

        public override void WriteContent(Stream stream) {
            var data = Encoding.GetBytes(ContentText);
            stream.Write(data, 0, data.Length);
        }
    }

    public class HtmlResponse : TextResponse {
        public HtmlResponse(string contents, string contentType = "text/html", Encoding encoding = null,
            StatusCode status = StatusCode.Ok)
            : base(contents, contentType, encoding, status) {
        }
    }
}