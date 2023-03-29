using System.Collections.Generic;
using System.Threading.Tasks;
using Rebus.Messages;
using Rebus.Routing;

namespace Rebus.Microsoft.Extensions.Configuration.Tests.Extensions;

static class RebusRouterExtensions
{
    public static Task<string> GetDestinationFor<TMessage>(this IRouter router) where TMessage : new() => router.GetDestinationAddress(new Message(new Dictionary<string, string>(), new TMessage()));
}