using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Collections.Generic;
using System.Reflection;

namespace DiGi.WebAPI.WindowsService.xUnit
{
    public partial class Facts
    {
        // MVC discovers public top-level controllers only; this adds the nested EnumControllerFixture explicitly, and
        // nothing else, so a generated document holds exactly its operations.
        private sealed class ControllerFeatureProviderFixture : IApplicationFeatureProvider<ControllerFeature>
        {
            public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
            {
                feature.Controllers.Add(typeof(EnumControllerFixture).GetTypeInfo());
            }
        }
    }
}
