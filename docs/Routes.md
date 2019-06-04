# Routes

`RouteServer<T>` supports simple route dispatching from custom class.

```csharp
using static CreamRoll.RouteServer;

void Main() {
	var counter = new Counter();
	var server = new RouteServer<Counter>(counter);
	server.StartAsync();
}

class Counter {
	int counter = 0;
	[Get("/")]
    public string Count() {
		return (++counter).ToString();
	}
}
```

## RouteAttribute

You should use one of `RouteAttribute` to your method to route request.
There are 7 route types exactly many as HTTP request methods:

- Delete
- Get
- Head
- Options
- Post
- Put
- Patch

These attributes are declared inside of class `RouteServer`, not `RouteServer<T>` because [a generic class cannot derive from attribute](https://stackoverflow.com/questions/294216/why-does-c-sharp-forbid-generic-attribute-types), so you should use like `[RouteServer.Get("/")]` or `using static CreamRoll.RouteServer`.

---

Method `RouteAttribute` associated must be dispatcher-knowable type. Or not, it will throw `RouteMethodTypeMismatchException`.
Return type of method must be one of next types:

- `bool`
- `string`
- `Task<bool>`
- `Task<string>`

If method returns `bool` or `Task<bool>`, its return type means 'does this route method handled request', so if it returns false than route dispatcher will look up next route that matches path rule.
If method return something else, its return type will be written to output stream and Content-Type of output will be `text/html` by default.

Parameters of method must shaped like one of nexts:

- `void`
- `RouteContext`

`RouteContext` contains `HttpListenerRequest` and `HttpListenerResponse` so that you can modify response or get detailed information from request yourself, but be careful, **if method returns string-like type, RouteServer<T> writes outputstream and close it after write method body to output stream**.
It also means if you return `bool` or `Task<bool>` you should write output to response manually.

---

Currently this project is working on process so any of these rules could be changed.

