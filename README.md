# Rebus.Microsoft.Extensions.Configuration

[![install from nuget](https://img.shields.io/nuget/v/Rebus.Microsoft.Extensions.Configuration.svg?style=flat-square)](https://www.nuget.org/packages/Rebus.Microsoft.Extensions.Configuration)

Provides a Microsoft Extensions Configuration integration for [Rebus](https://github.com/rebus-org/Rebus).

![](https://raw.githubusercontent.com/rebus-org/Rebus/master/artwork/little_rebusbus2_copy-200x200.png)

---

In the newer incarnations of the .NET Framework, you're encouraged to configure your things via [Microsoft.Extensions.Configuration](https://www.nuget.org/packages/Microsoft.Extensions.Configuration).

While there's no doubt that the name is pretty silly, it's actually neatly designed, since it allows for picking up configuration from many different sources.

With Rebus.Microsoft.Extensions.Configuration you can have Rebus pick up your endpoint mappings from `IConfiguration` 😎

Let's say you have a configuration file, `appsettings.json`, which looks like this:

```json
{
  "Mappings": {
    "TestApp.Messages": "testapp-queue",
    "AnotherApp.Messages": "anotherapp-queue"
  }
}
```

If you're using Microsoft's generic host, then you should include the [Rebus.ServiceProvider](https://www.nuget.org/packages/Rebus.ServiceProvider) package too, because then you can configure your bus instance simply like this:

```csharp
var host = Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) =>
    {
        services.AddRebus(
            configure => configure
                .Transport(t => t.UseInMemoryTransport(new InMemNetwork(), "my-queue"))
                .Routing(r => r.TypeBased().AddMappingsFromConfiguration(context.Configuration, "Endpoints"))
        );
    })
    .Build();

host.Run();
```

If you are in place where the generic host is not available, you can still use the package in a slightly more manual way.

Start out by loading up your configuration file like this:

```csharp
// build configuration
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();


```

If we then want Rebus to pick up the endpoint mapping from it, we can do it like this:

```csharp
Configure.With(...)
    .Transport(...)
    .Routing(r => r.TypeBased().AddMappingsFromConfiguration(configuration, "Mappings"))
    .Start();

```

As you can see, we reference the `Mappings` object in the JSON by specifying it as an argument to `AddMappingsFromConfiguration`.

Since configuration can be loaded from many different sources, you just need to ensure that your mappings can be bound to a `Dictionary<string, string>`.

It even works with environment variables! You can make Microsoft.Extensions.Configuration load your environment variables like this:

```charp
var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .Build();

```

and then, if you configured the following variable:

```
RebusMappings:TestApp.Messages = some-queue
```

then it can be picked up like this:

```csharp
Configure.With(...)
    .Transport(...)
    .Routing(r => r.TypeBased().AddMappingsFromConfiguration(configuration, "RebusMappings"))
    .Start();
```

which is super neat. 🙂