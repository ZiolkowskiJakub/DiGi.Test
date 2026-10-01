using DiGi.GIS.WebAPI.Classes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace DiGi.GIS.WebAPI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Asserts every query parameter a request cannot be answered correctly without carries <see cref="BindRequiredAttribute"/>.
        /// <para>The attribute is the only machine-readable "required" signal the served OpenAPI document has: Swashbuckle reads it off the controller parameter, and ASP.NET Core turns its absence into a model-state error - HTTP 400 - instead of silently binding <c>default(T)</c>. A non-nullable value type parameter (a coordinate, a county id, a lattice spacing) is the case that used to succeed with the wrong scope, so it is what these assertions exist to pin.</para>
        /// </summary>
        [Fact]
        public void GisControllers_MandatoryQueryParameters_AreBindRequired()
        {
            (Type Type, string Method, string Parameter)[] parameters =
            [
                (typeof(AdministrativeAreal2DController), nameof(AdministrativeAreal2DController.GetAdministrativeAreal2DReferenceByCodeAsync), "code"),
                (typeof(AdministrativeAreal2DController), nameof(AdministrativeAreal2DController.GetAdministrativeAreal2DReferenceByIdAsync), "id"),
                (typeof(AdministrativeAreal2DController), nameof(AdministrativeAreal2DController.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync), "administrativeArealType"),
                (typeof(AdministrativeAreal2DController), nameof(AdministrativeAreal2DController.GetIdsByCodeAsync), "code"),
                (typeof(AdministrativeAreal2DController), nameof(AdministrativeAreal2DController.GetIdsByAdministrativeArealTypeAsync), "administrativeArealType"),
                (typeof(AdministrativeAreal2DController), nameof(AdministrativeAreal2DController.GetSubCodesAsync), "code"),
                (typeof(BuildingController), nameof(BuildingController.GetItemsByReferenceAsync), "reference"),
                (typeof(BuildingController), nameof(BuildingController.GetItemByReferenceAsync), "countyId"),
                (typeof(Building2DController), nameof(Building2DController.GetBuilding2DReferenceByIdAsync), "id"),
                (typeof(Building2DController), nameof(Building2DController.GetReferencesByCountyIdAsync), "countyId"),
                (typeof(BuildingModelController), nameof(BuildingModelController.GetCountByCountyIdAsync), "countyId"),
                (typeof(BuildingModelController), nameof(BuildingModelController.GetItemsByCircleAsync), "x"),
                (typeof(BuildingModelController), nameof(BuildingModelController.GetItemsByReferencesAsync), "references"),
                (typeof(BuildingDataController), nameof(BuildingDataController.GetCountyIdsByReferenceAsync), "reference"),
                (typeof(BuildingDataController), nameof(BuildingDataController.GetUniqueValuesAsync), "columnUniqueId"),
                (typeof(BuildingDataController), nameof(BuildingDataController.UpdateItemsByCountyIdsAsync), "countyIds"),
                (typeof(EPWFileController), nameof(EPWFileController.GetEPWFileAsync), "y"),
                (typeof(OrtoDatasController), nameof(OrtoDatasController.GetEstimatedCoverageFactorAsync), "administrativeAreal2DId"),
                (typeof(OrtoDatasController), nameof(OrtoDatasController.GetImageByReferenceAsync), "year"),
                (typeof(OrtoDatasController), nameof(OrtoDatasController.UpdateItemsByCodeAsync), "code"),
                (typeof(TerrainController), nameof(TerrainController.GetMesh3DByCircleAsync), "x"),
                (typeof(TerrainController), nameof(TerrainController.GetMesh3DByBoundingBoxAsync), "y_2"),
                (typeof(TerrainController), nameof(TerrainController.GetDensitiesByCountyIdsAsync), "countyIds"),
                (typeof(TerrainController), nameof(TerrainController.GetCoverageByCountyIdAsync), "gridSize"),
                (typeof(UnitController), nameof(UnitController.GetComplianceAsync), "administrativeArealType"),
                (typeof(UnitController), nameof(UnitController.GetItemByIdAsync), "id"),
                (typeof(YearBuiltDataController), nameof(YearBuiltDataController.GetCountByCountyIdAsync), "countyId")
            ];

            Assert.NotEmpty(parameters);

            foreach ((Type type, string method, string parameter) in parameters)
            {
                ParameterInfo parameterInfo = FindQueryParameter(type, method, parameter);

                Assert.NotNull(parameterInfo.GetCustomAttribute<FromQueryAttribute>());
                Assert.NotNull(parameterInfo.GetCustomAttribute<BindRequiredAttribute>());
            }
        }

        /// <summary>
        /// Asserts the query parameters whose absence is meaningful stay optional, so no caller that legitimately omits one is turned into HTTP 400.
        /// <para>Named explicitly rather than derived, because the classification is a per-endpoint decision: a county identifier that means "every county" when omitted, a type filter that may be left off, and the lattice origin whose documented default is zero all look like a mandatory selector to a rule and are not.</para>
        /// </summary>
        [Fact]
        public void GisControllers_OptionalQueryParameters_AreNotBindRequired()
        {
            (Type Type, string Method, string Parameter)[] parameters =
            [
                (typeof(AdministrativeAreal2DController), nameof(AdministrativeAreal2DController.GetAdministrativeAreal2DReferenceByCodeAsync), "administrativeArealType"),
                (typeof(AdministrativeAreal2DController), nameof(AdministrativeAreal2DController.GetAdministrativeAreal2DReferencesByCodeAsync), "administrativeArealType"),
                (typeof(AdministrativeAreal2DController), nameof(AdministrativeAreal2DController.GetIdByCodeAsync), "administrativeArealType"),
                (typeof(BuildingController), nameof(BuildingController.GetCountAsync), "countyId"),
                (typeof(BuildingController), nameof(BuildingController.GetItemByReferenceAsync), "x"),
                (typeof(Building2DController), nameof(Building2DController.GetCountyPartMismatchesAsync), "code"),
                (typeof(OrtoDatasController), nameof(OrtoDatasController.GetRandomBuilding2DReferenceAsync), "countyIds"),
                (typeof(TerrainController), nameof(TerrainController.GetMesh3DByCircleAsync), "tolerance"),
                (typeof(TerrainController), nameof(TerrainController.GetCoverageByCountyIdAsync), "originX"),
                (typeof(TerrainController), nameof(TerrainController.GetCoverageByCountyIdAsync), "originY"),
                (typeof(TerrainController), nameof(TerrainController.GetSummariesByCountyIdsAsync), "countyIds")
            ];

            Assert.NotEmpty(parameters);

            foreach ((Type type, string method, string parameter) in parameters)
            {
                ParameterInfo parameterInfo = FindQueryParameter(type, method, parameter);

                Assert.Null(parameterInfo.GetCustomAttribute<BindRequiredAttribute>());
            }
        }

        /// <summary>
        /// Asserts the terrain mesh circle carries the radius and diameter ceilings as schema constraints.
        /// <para>The action rejects a radius beyond <see cref="Constants.Terrain.MaximumRadius"/> at runtime, but without the attribute a client generator reads an unbounded number. The diameter is capped at twice the radius, because it is halved before the same check.</para>
        /// </summary>
        [Fact]
        public void TerrainController_TerrainMeshRadii_CarryMaximum()
        {
            RangeAttribute? rangeAttribute_Radius = FindRange(typeof(TerrainController), nameof(TerrainController.GetMesh3DByCircleAsync), "radius");
            Assert.NotNull(rangeAttribute_Radius);
            Assert.Equal(Constants.Terrain.MaximumRadius, System.Convert.ToDouble(rangeAttribute_Radius.Maximum));

            RangeAttribute? rangeAttribute_Diameter = FindRange(typeof(TerrainController), nameof(TerrainController.GetMesh3DByCircleAsync), "diameter");
            Assert.NotNull(rangeAttribute_Diameter);
            Assert.Equal(2 * Constants.Terrain.MaximumRadius, System.Convert.ToDouble(rangeAttribute_Diameter.Maximum));
        }

        private static ParameterInfo FindQueryParameter(Type type, string method, string parameter)
        {
            MethodInfo? methodInfo = type.GetMethod(method, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            Assert.NotNull(methodInfo);

            ParameterInfo? parameterInfo = Array.Find(methodInfo!.GetParameters(), x => string.Equals(x.Name, parameter, StringComparison.Ordinal));
            Assert.NotNull(parameterInfo);

            return parameterInfo!;
        }

        private static RangeAttribute? FindRange(Type type, string method, string parameter)
        {
            return FindQueryParameter(type, method, parameter).GetCustomAttribute<RangeAttribute>();
        }
    }
}
