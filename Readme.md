# CreamRoll

> WIP

CreamRoll is lightweight, easy framework for building HTTP based sync/async service on .NET Standard.

Inspired from [aardwolf](https://github.com/JamesDunne/aardwolf) and [Nancy](http://nancyfx.org/).

## Example

Currently route interface looks like:

```csharp
class MainServer : RouteServer {
	public MainServer() {
		Get("/", async ctx => {
			ctx.Response.ContentType = "text/html";
			var writer = new StreamWriter(ctx.Response.OutputStream);
			await writer.WriteLineAsync("<h1>Hello, World!");
			await writer.FlushAsync();
			writer.Close();
			return true;
		}
	}
}
```

Soon, it will be:

```csharp
[Get("/")]
public string Root() {
	return "<marquee>Hello, World!";
}
```

And also supports parameterized path DSL using dynamic:

```csharp
[Get("/api/user/{name}?{detail?:bool}", ContentType = Json)]
public async Task<string> GetUserInfoAsync(RollContext ctx) {
	var user = await GetUser(ctx.query.name);
	var result = user.GetInfo(ctx.query.detail ?? false)

	return result.ToString();
}
```

