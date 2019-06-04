# CreamRoll

[![Build](https://img.shields.io/appveyor/ci/phillyai/CreamRoll/master.svg)](https://ci.appveyor.com/project/phillyai/CreamRoll)
[![Nuget](https://img.shields.io/nuget/dt/CreamRoll.svg)](https://www.nuget.org/packages/CreamRoll/)

> WIP

CreamRoll is lightweight, easy framework for building HTTP based sync/async service on .NET Standard.

Inspired from [aardwolf](https://github.com/JamesDunne/aardwolf) and [Nancy](http://nancyfx.org/).

## Example

So easy even a cat can do it

```csharp
[Get("/")]
public string Hello() {
	return "<marquee>Hello, World!";
}

[Get("/async")]
public Task<string> HelloAsync() {
	return Task.FromResult("<marquee>Hello, World!");
}
```

And also supports parameterized path DSL using dynamic (WIP):

```csharp
[Get("/api/user/{name}?{detail?:bool}", ContentType = Json)]
public async Task<string> GetUserInfoAsync(RollContext ctx) {
	var user = await GetUser(ctx.query.name);
	var result = user.GetInfo(ctx.query.detail ?? false)

	return result.ToString();
}
```

