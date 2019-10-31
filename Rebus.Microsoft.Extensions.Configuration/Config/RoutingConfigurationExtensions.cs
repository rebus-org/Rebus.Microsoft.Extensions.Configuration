using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Rebus.Exceptions;
using Rebus.Routing.TypeBased;

namespace Rebus.Config
{
    /// <summary>
    /// Configuration extensions
    /// </summary>
    public static class RoutingConfigurationExtensions
    {
        /// <summary>
        /// Adds mappings
        /// </summary>
        public static TypeBasedRouterConfigurationExtensions.TypeBasedRouterConfigurationBuilder AddMappingsFromConfiguration(this TypeBasedRouterConfigurationExtensions.TypeBasedRouterConfigurationBuilder builder, IConfiguration configuration, string path = null)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            var mappings = new Dictionary<string, string>();

            if (path == null)
            {
                configuration.Bind(mappings);
            }
            else
            {
                var section = configuration.GetSection(path) ?? throw new RebusConfigurationException(
                                  $"Could not find section with path '{path}' in the current configuration (which was very unexpected, because the configuration library should return an empty section and NOT null when looking for non-existent keys....)");
                section.Bind(mappings);
            }

            SetUpEndpointMappings(mappings, (type, queue) => builder.Map(type, queue));

            return builder;
        }

        static void SetUpEndpointMappings(Dictionary<string, string> mappings, Action<Type, string> mappingFunction)
        {
            var mappingElements = mappings.OrderBy(c => !IsAssemblyName(c.Key)).ToList();

            foreach (var element in mappingElements)
            {
                if (IsAssemblyName(element.Key))
                {
                    var assemblyName = element.Key;
                    var assembly = LoadAssembly(assemblyName);

                    foreach (var type in assembly.GetTypes())
                    {
                        mappingFunction(type, element.Value);
                    }
                }
                else
                {
                    var typeName = element.Key;
                    var messageType = Type.GetType(typeName);

                    if (messageType == null)
                    {
                        throw new RebusConfigurationException($@"Could not find the message type {typeName}. If you choose to map a specific message type, please ensure that the type is available for Rebus to load. This requires that the assembly can be found in Rebus' current runtime directory, that the type is available, and that any (of the optional) version and key requirements are matched");
                    }

                    mappingFunction(messageType, element.Value);
                }
            }
        }

        static bool IsAssemblyName(string key) => !key.Contains(",");

        static Assembly LoadAssembly(string assemblyName)
        {
            try
            {
                return Assembly.Load(assemblyName);
            }
            catch (Exception exception)
            {
                throw new RebusConfigurationException($@"Something went wrong when trying to load message types from assembly {assemblyName}

{exception}

For this to work, Rebus needs access to an assembly with one of the following filenames:
    {assemblyName}.dll
    {assemblyName}.exe
");
            }
        }
    }

}