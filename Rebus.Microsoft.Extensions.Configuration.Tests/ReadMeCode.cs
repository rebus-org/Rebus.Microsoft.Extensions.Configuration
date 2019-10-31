using System;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Rebus.Activation;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using Rebus.Transport.InMem;

namespace Rebus.Microsoft.Extensions.Configuration.Tests
{
    [TestFixture]
    public class ReadMeCode
    {
        [Test]
        public void Wat()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var activator = new BuiltinHandlerActivator();

            Configure.With(activator)
                .Transport(t => t.UseInMemoryTransport(new InMemNetwork(), "my-queue"))
                .Routing(r => r.TypeBased().AddMappingsFromConfiguration(configuration, "Endpoints"))
                .Start();
        }

        [Test]
        public void PrintJson()
        {
            var config = new JObject
            {
                {
                    "Mappings", new JObject
                    {
                        {"TestApp.Messages", "testapp-queue" },
                        {"AnotherApp.Messages", "anotherapp-queue" }
                    }
                }
            };
            Console.WriteLine(config.ToString(Formatting.Indented));
        }
    }
}