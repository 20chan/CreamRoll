using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace CreamRoll.Tests {
    public static class WebClientHelper {
        public static string Body(string url, string method, string body = null) {
            var response = Request(url, method, body);
            var stream = response.GetResponseStream();
            var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        public static int StatusCode(string url, string method) {
            return (int)Request(url, method).StatusCode;
        }

        public static HttpWebResponse Request(string url, string method, string body = null) {
            var req = WebRequest.Create(url);
            req.Method = method;

            if (body != null) {
                var reqStream = req.GetRequestStream();
                var writer = new StreamWriter(reqStream);
                writer.Write(body);
                writer.Close();
            }

            HttpWebResponse response;
            try {
                response = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException ex) {
                response = (HttpWebResponse)ex.Response;
            }
            return response;
        }
    }
}
