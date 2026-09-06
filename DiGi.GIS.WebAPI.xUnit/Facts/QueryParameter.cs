using DiGi.GIS.PostgreSQL.Enums;
using DiGi.GIS.WebAPI.Classes;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Reflection;

namespace DiGi.GIS.WebAPI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Asserts that the <c>GetItemsByPointAsync</c> method in <see cref="AdministrativeAreal2DController"/> binds the <see cref="AdministrativeArealType"/> parameter using the query parameter name <c>administrativearealtype</c>.
        /// <para>This guards against parameter name mismatches where consumers pass <c>?administrativearealtype=...</c> but the endpoint binds to a different query parameter name.</para>
        /// </summary>
        [Fact]
        public void AdministrativeAreal2DController_GetItemsByPointAsync_QueryParameterBinding()
        {
            MethodInfo? methodInfo = typeof(AdministrativeAreal2DController).GetMethod(nameof(AdministrativeAreal2DController.GetItemsByPointAsync));
            Assert.NotNull(methodInfo);

            ParameterInfo? parameterInfo = null;
            foreach (ParameterInfo parameterInfo_Temp in methodInfo.GetParameters())
            {
                if (parameterInfo_Temp.Name == "administrativeArealType")
                {
                    parameterInfo = parameterInfo_Temp;
                    break;
                }
            }

            Assert.NotNull(parameterInfo);

            FromQueryAttribute? fromQueryAttribute = parameterInfo.GetCustomAttribute<FromQueryAttribute>();
            Assert.NotNull(fromQueryAttribute);
            Assert.Equal("administrativearealtype", fromQueryAttribute.Name);
        }

        /// <summary>
        /// Asserts that all action methods in <see cref="AdministrativeAreal2DController"/> with an <see cref="AdministrativeArealType"/> parameter consistently bind using the query parameter name <c>administrativearealtype</c>.
        /// </summary>
        [Fact]
        public void AdministrativeAreal2DController_AdministrativeArealType_QueryParameterBindings()
        {
            MethodInfo[] methodInfos = typeof(AdministrativeAreal2DController).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (MethodInfo methodInfo in methodInfos)
            {
                foreach (ParameterInfo parameterInfo in methodInfo.GetParameters())
                {
                    Type parameterType = parameterInfo.ParameterType;
                    Type? underlyingType = Nullable.GetUnderlyingType(parameterType);

                    if (parameterType == typeof(AdministrativeArealType) || underlyingType == typeof(AdministrativeArealType))
                    {
                        FromQueryAttribute? fromQueryAttribute = parameterInfo.GetCustomAttribute<FromQueryAttribute>();
                        if (fromQueryAttribute != null)
                        {
                            Assert.Equal("administrativearealtype", fromQueryAttribute.Name);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Asserts that the Building2D write endpoint taking county parts also accepts the county code, bound as <c>code</c>.
        /// <para>Naming several parts leaves the part of each building to geometry, and the code is what narrows that decision to the parts of one county. Without it a building of a multi-part county is tested against every county whose extent reaches it - one query returning whole county polygons per building - which is the difference between minutes and hours over a county of tens of thousands of buildings.</para>
        /// <para>The <c>updateitems</c> action resolves a code to its parts and then delegates here, so this parameter is what carries the code across that delegation. It went missing once, and nothing failed: the import stayed correct and became slow.</para>
        /// </summary>
        [Fact]
        public void Building2DController_UpdateItemsByCountyIdsAsync_CodeQueryParameterBinding()
        {
            MethodInfo? methodInfo = typeof(Building2DController).GetMethod(nameof(Building2DController.UpdateItemsByCountyIdsAsync));
            Assert.NotNull(methodInfo);

            ParameterInfo? parameterInfo = null;
            foreach (ParameterInfo parameterInfo_Temp in methodInfo.GetParameters())
            {
                if (parameterInfo_Temp.Name == "code")
                {
                    parameterInfo = parameterInfo_Temp;
                    break;
                }
            }

            Assert.NotNull(parameterInfo);

            FromQueryAttribute? fromQueryAttribute = parameterInfo.GetCustomAttribute<FromQueryAttribute>();
            Assert.NotNull(fromQueryAttribute);
            Assert.Equal("code", fromQueryAttribute.Name);

            // Optional, so every client that already posts without it keeps working.
            Assert.True(parameterInfo.IsOptional);

            ParameterInfo[] parameterInfos = methodInfo.GetParameters();
            Assert.Equal(typeof(System.Threading.CancellationToken), parameterInfos[^1].ParameterType);
        }
    }
}
