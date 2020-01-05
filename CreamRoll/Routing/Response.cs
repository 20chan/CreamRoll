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

        protected string _contentType;
        public virtual string ContentType {
            get => _contentType;
            set {
                _contentType = value ?? "text/plain";
                Headers.ContentType = _contentType;
            }
        }

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

        public override string ContentType {
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

    public class JsonResponse : TextResponse {
        public JsonResponse(string contents, string contentType = "application/json", Encoding encoding = null,
            StatusCode status = StatusCode.Ok)
            : base(contents, contentType, encoding, status) {
        }
    }

    public class FileResponse : Response {
        private static IDictionary<string, string> _mimeTypeMappings =
            new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) {
                { ".asf", "video/x-ms-asf" },
                { ".asx", "video/x-ms-asf" },
                { ".avi", "video/x-msvideo" },
                { ".bin", "application/octet-stream" },
                { ".cco", "application/x-cocoa" },
                { ".crt", "application/x-x509-ca-cert" },
                { ".css", "text/css" },
                { ".deb", "application/octet-stream" },
                { ".der", "application/x-x509-ca-cert" },
                { ".dll", "application/octet-stream" },
                { ".dmg", "application/octet-stream" },
                { ".ear", "application/java-archive" },
                { ".eot", "application/octet-stream" },
                { ".exe", "application/octet-stream" },
                { ".flv", "video/x-flv" },
                { ".gif", "image/gif" },
                { ".hqx", "application/mac-binhex40" },
                { ".htc", "text/x-component" },
                { ".htm", "text/html" },
                { ".html", "text/html" },
                { ".ico", "image/x-icon" },
                { ".img", "application/octet-stream" },
                { ".iso", "application/octet-stream" },
                { ".jar", "application/java-archive" },
                { ".jardiff", "application/x-java-archive-diff" },
                { ".jng", "image/x-jng" },
                { ".jnlp", "application/x-java-jnlp-file" },
                { ".jpeg", "image/jpeg" },
                { ".jpg", "image/jpeg" },
                { ".js", "application/x-javascript" },
                { ".mml", "text/mathml" },
                { ".mng", "video/x-mng" },
                { ".mov", "video/quicktime" },
                { ".mp3", "audio/mpeg" },
                { ".mpeg", "video/mpeg" },
                { ".mpg", "video/mpeg" },
                { ".msi", "application/octet-stream" },
                { ".msm", "application/octet-stream" },
                { ".msp", "application/octet-stream" },
                { ".pdb", "application/x-pilot" },
                { ".pdf", "application/pdf" },
                { ".pem", "application/x-x509-ca-cert" },
                { ".pl", "application/x-perl" },
                { ".pm", "application/x-perl" },
                { ".png", "image/png" },
                { ".prc", "application/x-pilot" },
                { ".ra", "audio/x-realaudio" },
                { ".rar", "application/x-rar-compressed" },
                { ".rpm", "application/x-redhat-package-manager" },
                { ".rss", "text/xml" },
                { ".run", "application/x-makeself" },
                { ".sea", "application/x-sea" },
                { ".shtml", "text/html" },
                { ".sit", "application/x-stuffit" },
                { ".swf", "application/x-shockwave-flash" },
                { ".tcl", "application/x-tcl" },
                { ".tk", "application/x-tcl" },
                { ".txt", "text/plain" },
                { ".war", "application/java-archive" },
                { ".wbmp", "image/vnd.wap.wbmp" },
                { ".wmv", "video/x-ms-wmv" },
                { ".xml", "text/xml" },
                { ".xpi", "application/x-xpinstall" },
                { ".zip", "application/zip" },
            };

        public string FilePath;

        public FileResponse(string path) {
            FilePath = path;
            if (_mimeTypeMappings.TryGetValue(Path.GetExtension(path), out var type)) {
                ContentType = type;
            }
        }

        public override void WriteContent(Stream stream) {
            using (var file = File.Open(FilePath, FileMode.Open)) {
                file.CopyTo(stream);
            }
        }
    }
}