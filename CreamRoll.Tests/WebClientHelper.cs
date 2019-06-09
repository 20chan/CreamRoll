using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace CreamRoll.Tests {
    public static class WebClientHelper {
        public static string SendClient(string url, string method) {
            var req = WebRequest.Create(url);
            req.Timeout = 1000;
            req.Method = method;

            var response = req.GetResponse();
            var stream = response.GetResponseStream();
            var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
        public static string SendClient(string url, string method, string body) {
            var req = WebRequest.Create(url);
            req.Method = method;

            var reqStream = req.GetRequestStream();
            var writer = new StreamWriter(reqStream);
            writer.Write(body);
            writer.Close();

            var response = req.GetResponse();
            var stream = response.GetResponseStream();
            var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
