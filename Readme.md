# CreamRoll

[![Build](https://img.shields.io/appveyor/ci/phillyai/CreamRoll/master.svg)](https://ci.appveyor.com/project/phillyai/CreamRoll)
[![Test](https://img.shields.io/appveyor/tests/phillyai/CreamRoll/master.svg)](https://ci.appveyor.com/project/phillyai/CreamRoll)
[![Version](https://img.shields.io/nuget/v/CreamRoll.svg)](https://www.nuget.org/packages/CreamRoll/)
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

And also supports parameterized path DSL:

```csharp
[Get("/api/user/{name}/info")]
public async Task<string> GetUserInfoAsync(RollContext ctx) {
	var user = await GetUser(ctx.Query.name);
	return user.Info;
}
```

