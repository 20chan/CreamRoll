using System;
using System.Threading.Tasks;
using CreamRoll;
using CreamRoll.Routing;
using Xunit;

namespace CreamRoll.Tests {
    public class RouteServerTest : IDisposable {
        int port = 4141;
        TestServer server;
        RouteServer runner;
        public RouteServerTest() {
            server = new TestServer();
            runner = new RouteServer(port: port);
            runner.AppendRoutes(server);
            runner.HandleErrorToConsole = false;
            runner.StartAsync();
        }

        ~RouteServerTest() {
            Dispose();
        }

        public void Dispose() {
            runner.Stop();
        }

        [Fact]
        public void TestBasicResponse() {
            Assert.Equal("hello world!", GET("/"));
        }
        
        [Fact]
        public void TestBasicAsyncResponse() {
            Assert.Equal("hello world!", GET("/async"));
        }

        [Fact]
        public void Test404() {
            Assert.Equal(404, GETStatus("/unkonwnpath"));
        }

        string GET(string path)
            => WebClientHelper.Body($"http://localhost:{port}{path}", "GET");

        int GETStatus(string path)
            => WebClientHelper.StatusCode($"http://localhost:{port}{path}", "GET");

        class TestServer {
            [Get("/")]
            public Response Root(Request req) {
                return new TextResponse("hello world!");
            }

            [Get("/async")]
            public Task<Response> Async(Request req) {
                return Task.FromResult<Response>(new TextResponse("hello world!"));
            }
        }
    }
}
