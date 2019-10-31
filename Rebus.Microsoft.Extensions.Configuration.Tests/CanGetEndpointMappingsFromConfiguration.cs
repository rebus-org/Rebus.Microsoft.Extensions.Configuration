using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Rebus.Activation;
using Rebus.Config;
using Rebus.Exceptions;
using Rebus.Microsoft.Extensions.Configuration.Tests.Extensions;
using Rebus.Routing;
using Rebus.Routing.TypeBased;
using Rebus.Transport.InMem;
using TestApp.Messages;

namespace Rebus.Microsoft.Extensions.Configuration.Tests
{
    [TestFixture]
    public class CanGetEndpointMappingsFromConfiguration
    {
        [Test]
        public async Task AddMappingsForTypes()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "Data", "appsettings_types.json"))
                .Build();

            var router = GetRouter(configuration);

            Assert.That(await router.GetDestinationFor<FirstMessage>(), Is.EqualTo("some-queue"));
            Assert.That(await router.GetDestinationFor<SecondMessage>(), Is.EqualTo("another-queue"));
            Assert.That(await router.GetDestinationFor<ThirdMessage>(), Is.EqualTo("yet-another-queue"));
        }

        [Test]
        public async Task AddMappingsForEntireAssembly()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "Data", "appsettings_assembly.json"))
                .Build();

            var router = GetRouter(configuration);

            Assert.That(await router.GetDestinationFor<FirstMessage>(), Is.EqualTo("some-queue"));
            Assert.That(await router.GetDestinationFor<SecondMessage>(), Is.EqualTo("some-queue"));
            Assert.That(await router.GetDestinationFor<ThirdMessage>(), Is.EqualTo("some-queue"));
        }

        [Test]
        public async Task ThrowsWhen_TypeCannotBeFound()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "Data", "appsettings_wrong_type.json"))
                .Build();

            var exception = Assert.Throws<RebusConfigurationException>(() => GetRouter(configuration));

            Console.WriteLine(exception);
        }

        [Test]
        public async Task ThrowsWhen_AssemblyCannotBeFound()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "Data", "appsettings_wrong_assembly.json"))
                .Build();

            var exception = Assert.Throws<RebusConfigurationException>(() => GetRouter(configuration));

            Console.WriteLine(exception);
        }

        static IRouter GetRouter(IConfigurationRoot configuration)
        {
            IRouter router = null;

            using (var activator = new BuiltinHandlerActivator())
            {
                Configure.With(activator)
                    .Transport(t => t.UseInMemoryTransport(new InMemNetwork(), "test-queue"))
                    .Routing(r =>
                    {
                        r.TypeBased().AddMappingsFromConfiguration(configuration, "EndpointMappings");

                        r.Decorate(c => router = c.Get<IRouter>());
                    })
                    .Start();
            }

            return router;
        }
    }
}
