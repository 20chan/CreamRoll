# CreamRoll

[![Build](https://img.shields.io/appveyor/ci/20chan/CreamRoll/master.svg)](https://ci.appveyor.com/project/20chan/CreamRoll)
[![Test](https://img.shields.io/appveyor/tests/20chan/CreamRoll/master.svg)](https://ci.appveyor.com/project/20chan/CreamRoll)
[![Version](https://img.shields.io/nuget/v/CreamRoll.svg)](https://www.nuget.org/packages/CreamRoll/)
[![Nuget](https://img.shields.io/nuget/dt/CreamRoll.svg)](https://www.nuget.org/packages/CreamRoll/)

![logo](logo.png)

CreamRoll is lightweight, easy framework for building HTTP based async service on .NET Standard.

Inspired from [aardwolf](https://github.com/JamesDunne/aardwolf) and [Nancy](http://nancyfx.org/).

So easy even a cat can do it

## Example

> Basic Usage

```csharp
var a = new A();
var b = new B();
var server = new RouteServer(port: 4001);
server.AppendRoutes(a);
server.AppendRoutes(b, "/baseuri/forexample");

server.StartAsync();

class A {
    [Get("/")]
    public Response SomeMethod(Request req) {
        return new TextResponse("it works");
    }
}
```

> Basic routing

```csharp
[Get("/")]
public Response Hello(Request req) {
	return new Response("<marquee>Hello, World!");
}

[Get("/async")]
public Task<Response> HelloAsync(Request req) {
	return Task.FromResult(new Response("<marquee>Hello, World!"));
}
```

> Parameterized path matching DSL

```csharp
[Get("/api/users/{id:int}/info")]
public async Task<Response> GetUserInfoAsync(Request req) {
    var id = int.Parse(req.Query.id)
	var user = await GetUserById(id);
	return new Response(user.Info);
}

[Get("/api/users/{id:int}?{from}&{sort}=true")]
public async Task<Response> GetUserList(Request req) {
    throw new NotImplementException();
}
```

> Static file server example

```csharp
// i know it's horrible but anyway.. it works
[Get("/{f0}")]
[Get("/{f0}/{f1}")]
[Get("/{f0}/{f1}/{f2}")]
public Response RouteFile(Request req) {
    var path = req.AbsolutePathWithoutBaseUri.Substring(1);
    if (string.IsNullOrEmpty(path)) {
        path = Index;
    }

    var fullPath = Path.Combine(DirectoryPath, path);
    if (!File.Exists(fullPath)) {
        return new TextResponse("file not found", status: StatusCode.NotFound);
    }

    return new FileResponse(fullPath);
}
```

### Features

- [x] Both sync and async routes
- [x] Custom routes with BaseUri
- [x] Path / Query pattern matching
- [x] Path type filtering (integer only)
- [x] Multiple routes and pattern matching
- [ ] Pipelined request and responses
- [ ] Session plugin

### Docs

CreamRoll is under developments.
todo
