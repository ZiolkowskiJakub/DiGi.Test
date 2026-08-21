using DiGi.GIS.WebAPI.Classes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DiGi.GIS.WebAPI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Asserts that every controller in <c>DiGi.GIS.WebAPI</c> can be activated by MVC.
        /// <para>MVC resolves controllers through <c>DefaultControllerActivator</c> to <c>ActivatorUtilities.CreateFactory(type, Type.EmptyTypes)</c>. With no explicit argument types every public constructor matches, so a type declaring more than one throws <see cref="InvalidOperationException"/> at activation - before the action body runs, and therefore before any parameter guard can answer BadRequest.</para>
        /// <para>The failure never reaches a compiler or a route test: it surfaces only as HTTP 500 on every endpoint of the affected controller, which is how a second convenience constructor on <see cref="Building2DController"/> took the whole <c>gis/building2d</c> route out of service (issue #6). Building the factory here reproduces the exact activation path without a service provider or a database.</para>
        /// </summary>
        [Fact]
        public void ControllerActivation_SingleApplicableConstructor()
        {
            Assembly assembly = typeof(Building2DController).Assembly;

            List<Type> types =
            [
                .. assembly.GetTypes()
                    .Where(x => x.IsPublic && !x.IsAbstract && typeof(DiGi.WebAPI.Classes.WebAPIController).IsAssignableFrom(x))
                    .OrderBy(x => x.FullName)
            ];

            Assert.NotEmpty(types);

            foreach (Type type in types)
            {
                ConstructorInfo[] constructorInfos = type.GetConstructors();

                Assert.True(constructorInfos.Length == 1, $"{type.FullName} declares {constructorInfos.Length} public constructors. MVC cannot activate it - every endpoint would answer HTTP 500.");

                // Reproduces the activation path itself, not just the constructor count, so the test
                // keeps holding if the runtime ever changes how it picks between constructors.
                ObjectFactory objectFactory = ActivatorUtilities.CreateFactory(type, Type.EmptyTypes);

                Assert.NotNull(objectFactory);
            }
        }
    }
}
