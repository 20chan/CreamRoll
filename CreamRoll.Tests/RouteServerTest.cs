using System;
using System.Threading.Tasks;
using CreamRoll;
using Xunit;

namespace CreamRoll.Tests {
    public class RouteServerTest : IDisposable {
        int port = 4041;
        TestServer server;
        RouteServer<TestServer> runner;
        public RouteServerTest() {
            server = new TestServer();
            runner = new RouteServer<TestServer>(server, port: port);
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

        string GET(string path)
            => WebClientHelper.SendClient($"http://localhost:{port}{path}", "GET");

        class TestServer {
            [Get("/")]
            public string Root() {
                return "hello world!";
            }

            [Get("/async")]
            public Task<string> Async() {
                return Task.FromResult("hello world!");
            }
        }
    }
}
