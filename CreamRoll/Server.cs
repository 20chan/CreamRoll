using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace CreamRoll {
    public abstract class Server {
        public readonly string Host;
        public readonly int Port;

        public bool HandleErrorToConsole = true;

        private HttpListener listener;

        public Server(string host, int port, AuthenticationSchemes authScheme) {
            listener = new HttpListener();
            Host = host;
            Port = port;
            listener.AuthenticationSchemes = authScheme;

            listener.Prefixes.Add($"http://{host}:{Port}/");
        }

        public void StartAsync(int accepts = 4) {
            listener.Start();

            accepts *= Environment.ProcessorCount;
            var sem = new Semaphore(accepts, accepts);
            Task.Run(() => {
                while (listener.IsListening) {
                    sem.WaitOne();
                    listener.GetContextAsync().ContinueWith(async t => {
                        try {
                            sem.Release();

                            var ctx = await t;
                            await ProcessRequestAsync(ctx);
                        }
                        catch (Exception ex) {
                            if (!HandleErrorToConsole) {
                                throw;
                            }
                            await Console.Error.WriteAsync(ex.ToString());
                        }
                    });
                }
            });
        }

        protected abstract Task ProcessRequestAsync(HttpListenerContext ctx);

        public void Stop() {
            listener.Close();
        }
    }
}
