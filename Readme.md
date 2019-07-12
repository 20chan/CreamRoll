# CreamRoll

[![Build](https://img.shields.io/appveyor/ci/20chan/CreamRoll/master.svg)](https://ci.appveyor.com/project/20chan/CreamRoll)
[![Test](https://img.shields.io/appveyor/tests/20chan/CreamRoll/master.svg)](https://ci.appveyor.com/project/20chan/CreamRoll)
[![Version](https://img.shields.io/nuget/v/CreamRoll.svg)](https://www.nuget.org/packages/CreamRoll/)
[![Nuget](https://img.shields.io/nuget/dt/CreamRoll.svg)](https://www.nuget.org/packages/CreamRoll/)

> WIP

![logo](logo.png)

CreamRoll is lightweight, easy framework for building HTTP based async service on .NET Standard.

Inspired from [aardwolf](https://github.com/JamesDunne/aardwolf) and [Nancy](http://nancyfx.org/).

## Example

So easy even a cat can do it

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

And also supports parameterized path DSL:

```csharp
[Get("/api/user/{name}/info")]
public async Task<Response> GetUserInfoAsync(Request req) {
	var user = await GetUser(req.Query.name);
	return new Response(user.Info);
}
```

