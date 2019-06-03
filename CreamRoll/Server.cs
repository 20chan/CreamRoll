using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace CreamRoll {
    public abstract class Server {
        public readonly string Host;
        public readonly int Port;

        private HttpListener listener;

        public Server(string host = "*", int port = 80) {
            listener = new HttpListener();
            Host = host;
            Port = port;

            listener.Prefixes.Add($"http://{host}:{Port}/");
        }

        public void Start() {
            listener.Start();

            Task.Run(() => {
                while (listener.IsListening) {
                    try {
                        var ctx = listener.GetContext();
                        ProcessRequest(ctx);
                    }
                    catch (Exception ex) {
                        Console.Error.Write(ex.ToString());
                    }
                }
            });
        }

        public void StartAsync(int accepts = 4) {
            listener.Start();

            accepts *= Environment.ProcessorCount;
            var sem = new Semaphore(accepts, accepts);
            Task.Run(() => {
                while (listener.IsListening) {
                    sem.WaitOne();
                    listener.GetContextAsync().ContinueWith(async (t) => {
                        try {
                            sem.Release();

                            var ctx = await t;
                            await ProcessRequestAsync(ctx);
                        }
                        catch (Exception ex) {
                            await Console.Error.WriteAsync(ex.ToString());
                        }
                    });
                }
            });
        }

        protected virtual void ProcessRequest(HttpListenerContext ctx) {

        }

        protected virtual async Task ProcessRequestAsync(HttpListenerContext ctx) {

        }

        public void Stop() {
            listener.Close();
        }
    }
}