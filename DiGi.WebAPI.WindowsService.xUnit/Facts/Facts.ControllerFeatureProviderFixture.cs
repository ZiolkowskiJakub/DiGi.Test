using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DiGi.WebAPI.WindowsService.xUnit
{
    public partial class Facts
    {
        // MVC discovers public top-level controllers only; this adds the nested EnumControllerFixture explicitly, and
        // nothing else, so a generated document holds exactly its operations. Additional controller types - real
        // controllers from a loaded extension - join it, so a document can be generated through the operations an
        // extension actually serves.
        private sealed class ControllerFeatureProviderFixture : IApplicationFeatureProvider<ControllerFeature>
        {
            private readonly List<TypeInfo> types_Controller = [];

            public ControllerFeatureProviderFixture(Type[]? types_Controller = null)
            {
                this.types_Controller.Add(typeof(EnumControllerFixture).GetTypeInfo());
                if (types_Controller is not null)
                {
                    this.types_Controller.AddRange(types_Controller.Select(type => type.GetTypeInfo()));
                }
            }

            public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
            {
                foreach (TypeInfo type_Info in types_Controller)
                {
                    feature.Controllers.Add(type_Info);
                }
            }
        }
    }
}
