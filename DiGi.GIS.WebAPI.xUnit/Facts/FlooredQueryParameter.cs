using DiGi.GIS.WebAPI.Classes;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace DiGi.GIS.WebAPI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Asserts every query parameter whose action guard already refuses a value below a floor carries <see cref="MinimumAttribute"/> with exactly that floor.
        /// <para>The attribute is what moves the guard's decision to binding - a violation is a model-state error that <c>[ApiController]</c> answers with HTTP 400 before the action runs - and what <c>ParameterMinimumSchemaFilter</c> reads into the served document's <c>minimum</c>. A floor of 0 mirrors a <c>&lt; 0</c> guard and a floor of 1 a <c>&lt;= 0</c> guard, so the attribute never rejects a value the action accepts nor accepts one it rejects (ZiolkowskiJakub/DiGi.GIS.WebAPI#47).</para>
        /// </summary>
        [Fact]
        public void GisControllers_FlooredQueryParameters_CarryMinimum()
        {
            (Type Type, string Method, string Parameter, double Minimum, bool Exclusive)[] parameters =
            [
                // commandTimeout: floor 0 - the action guards commandTimeout < 0 (a value of 0 legitimately disables the timeout).
                (typeof(BuildingController), nameof(BuildingController.GetCountAsync), "commandTimeout", 0, false),
                (typeof(Building2DController), nameof(Building2DController.GetCountyPartMismatchesAsync), "commandTimeout", 0, false),
                (typeof(Building2DController), nameof(Building2DController.GetPoint2DsByAdministrativeAreal2DIdAsync), "commandTimeout", 0, false),
                (typeof(Building2DController), nameof(Building2DController.GetCentroidsByAdministrativeAreal2DIdAsync), "commandTimeout", 0, false),
                (typeof(Building2DController), nameof(Building2DController.GetReferenceDuplicatesAsync), "commandTimeout", 0, false),
                (typeof(Building2DController), nameof(Building2DController.GetReferencesByCountyIdAsync), "commandTimeout", 0, false),
                (typeof(Building2DController), nameof(Building2DController.GetReferenceUniquenessSummaryAsync), "commandTimeout", 0, false),
                (typeof(BuildingDataController), nameof(BuildingDataController.GetCategoriesAsync), "commandTimeout", 0, false),
                (typeof(BuildingDataController), nameof(BuildingDataController.GetColumnReferencesAsync), "commandTimeout", 0, false),
                (typeof(BuildingDataController), nameof(BuildingDataController.GetColumnsAsync), "commandTimeout", 0, false),
                (typeof(BuildingDataController), nameof(BuildingDataController.GetColumnsByCategoriesAsync), "commandTimeout", 0, false),
                (typeof(BuildingDataController), nameof(BuildingDataController.GetColumnsByCategoriesParameterAsync), "commandTimeout", 0, false),
                (typeof(BuildingDataController), nameof(BuildingDataController.GetColumnUniqueIdsAsync), "commandTimeout", 0, false),
                (typeof(BuildingDataController), nameof(BuildingDataController.GetCountByCountyIdAsync), "commandTimeout", 0, false),
                (typeof(BuildingDataController), nameof(BuildingDataController.GetCountyIdsByReferenceAsync), "commandTimeout", 0, false),
                (typeof(BuildingDataController), nameof(BuildingDataController.GetCoverageByCountyIdAsync), "commandTimeout", 0, false),
                (typeof(BuildingDataController), nameof(BuildingDataController.GetDuplicateReferencesAsync), "commandTimeout", 0, false),
                (typeof(BuildingDataController), nameof(BuildingDataController.GetHistogramSummaryAsync), "commandTimeout", 0, false),
                (typeof(BuildingDataController), nameof(BuildingDataController.GetMultivalueAggregateSummaryAsync), "commandTimeout", 0, false),
                (typeof(BuildingDataController), nameof(BuildingDataController.GetSinglevalueAggregateSummaryAsync), "commandTimeout", 0, false),
                (typeof(BuildingDataController), nameof(BuildingDataController.GetTableByBuildingDataByPagingParameterAsync), "commandTimeout", 0, false),
                (typeof(BuildingDataController), nameof(BuildingDataController.GetTableByBuildingDataByReferencesParameterAsync), "commandTimeout", 0, false),
                (typeof(BuildingDataController), nameof(BuildingDataController.GetTableByBuildingDataBySubdivisionIdsParameterAsync), "commandTimeout", 0, false),
                (typeof(BuildingDataController), nameof(BuildingDataController.GetTableByFilterGroupAsync), "commandTimeout", 0, false),
                (typeof(BuildingDataController), nameof(BuildingDataController.GetTableByReferenceAsync), "commandTimeout", 0, false),
                (typeof(BuildingDataController), nameof(BuildingDataController.GetUniqueValuesAsync), "commandTimeout", 0, false),
                (typeof(BuildingDataController), nameof(BuildingDataController.GetUniqueValuesByColumnUniqueIdParameterAsync), "commandTimeout", 0, false),
                (typeof(BuildingModelController), nameof(BuildingModelController.GetCountByCountyIdAsync), "commandTimeout", 0, false),
                (typeof(BuildingModelController), nameof(BuildingModelController.GetLatestCreatedAtByCountyIdAsync), "commandTimeout", 0, false),
                (typeof(OccupancyDataController), nameof(OccupancyDataController.GetBuilding2DDuplicateReferencesAsync), "commandTimeout", 0, false),
                (typeof(OccupancyDataController), nameof(OccupancyDataController.GetBuilding2DDuplicatesCountAsync), "commandTimeout", 0, false),
                (typeof(OrtoDatasController), nameof(OrtoDatasController.GetRandomBuilding2DReferenceAsync), "commandTimeout", 0, false),
                (typeof(OrtoDatasController), nameof(OrtoDatasController.GetCountByCountyIdAsync), "commandTimeout", 0, false),
                (typeof(TerrainController), nameof(TerrainController.GetCountByCountyIdAsync), "commandTimeout", 0, false),
                (typeof(YearBuiltDataController), nameof(YearBuiltDataController.GetReferencesByCountyIdAsync), "commandTimeout", 0, false),
                (typeof(YearBuiltDataController), nameof(YearBuiltDataController.GetCountByCountyIdAsync), "commandTimeout", 0, false),
                (typeof(YearBuiltDataController), nameof(YearBuiltDataController.GetItemsByReferencesAsync), "commandTimeout", 0, false),
                (typeof(YearBuiltDataController), nameof(YearBuiltDataController.GetReferenceDuplicatesAsync), "commandTimeout", 0, false),
                (typeof(YearBuiltDataController), nameof(YearBuiltDataController.GetCountyPartMismatchesAsync), "commandTimeout", 0, false),

                // limit: floor 1 - the duplicates endpoints guard limit <= 0.
                (typeof(Building2DController), nameof(Building2DController.GetReferenceDuplicatesAsync), "limit", 1, false),
                (typeof(BuildingDataController), nameof(BuildingDataController.GetDuplicateReferencesAsync), "limit", 1, false),
                (typeof(YearBuiltDataController), nameof(YearBuiltDataController.GetReferenceDuplicatesAsync), "limit", 1, false),
                (typeof(OccupancyDataController), nameof(OccupancyDataController.GetBuilding2DDuplicateReferencesAsync), "limit", 1, false),

                // limit: floor 0 - the lattice endpoints refuse limit < 0 in TryGetLatticeParameters.
                (typeof(TerrainController), nameof(TerrainController.GetCoverageByCountyIdAsync), "limit", 0, false),
                (typeof(TerrainController), nameof(TerrainController.GetGapsByBoundingBoxAsync), "limit", 0, false),

                // gridSize: the lattice endpoints refuse anything finer than MinimumGridSize; the density endpoint refuses gridSize <= 0 when it is supplied, an exclusive floor.
                (typeof(TerrainController), nameof(TerrainController.GetCoverageByCountyIdAsync), "gridSize", Constants.Terrain.MinimumGridSize, false),
                (typeof(TerrainController), nameof(TerrainController.GetGapsByBoundingBoxAsync), "gridSize", Constants.Terrain.MinimumGridSize, false),
                (typeof(TerrainController), nameof(TerrainController.GetDensitiesByCountyIdsAsync), "gridSize", 0, true),

                // the update queue guards count, claimTimeoutMinutes and maxAttempts against <= 0.
                (typeof(OrtoDatasController), nameof(OrtoDatasController.NextBuilding2DReferencesAsync), "count", 1, false),
                (typeof(OrtoDatasController), nameof(OrtoDatasController.NextBuilding2DReferencesAsync), "claimTimeoutMinutes", 1, false),
                (typeof(OrtoDatasController), nameof(OrtoDatasController.NextBuilding2DReferencesAsync), "maxAttempts", 1, false),

                // positive-identifier selectors: the action guards id/countyId/parentId/subdivisionId/administrativeAreal2DId against <= 0.
                (typeof(AdministrativeAreal2DController), nameof(AdministrativeAreal2DController.GetAdministrativeAreal2DReferenceByIdAsync), "id", 1, false),
                (typeof(AdministrativeAreal2DController), nameof(AdministrativeAreal2DController.GetAdministrativeAreal2DReferencePathByIdAsync), "id", 1, false),
                (typeof(AdministrativeAreal2DController), nameof(AdministrativeAreal2DController.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync), "parentId", 1, false),
                (typeof(AdministrativeAreal2DController), nameof(AdministrativeAreal2DController.GetItemByIdAsync), "id", 1, false),
                (typeof(Building2DController), nameof(Building2DController.GetPoint2DsByAdministrativeAreal2DIdAsync), "administrativeAreal2DId", 1, false),
                (typeof(Building2DController), nameof(Building2DController.GetCentroidsByAdministrativeAreal2DIdAsync), "administrativeAreal2DId", 1, false),
                (typeof(Building2DController), nameof(Building2DController.GetReferencesByCountyIdAsync), "countyId", 1, false),
                (typeof(Building2DController), nameof(Building2DController.GetReferencesByCountyIdAsync), "subdivisionId", 1, false),
                (typeof(OrtoDatasController), nameof(OrtoDatasController.GetOrtoDatasReferenceByReferenceAsync), "countyId", 1, false),
                (typeof(OrtoDatasController), nameof(OrtoDatasController.GetOrtoDatasReferencesByReferencesAsync), "countyId", 1, false),
                (typeof(OrtoDatasController), nameof(OrtoDatasController.GetOrtoDatasReferencesByCountyIdAsync), "countyId", 1, false),
                (typeof(YearBuiltDataController), nameof(YearBuiltDataController.GetReferencesByCountyIdAsync), "countyId", 1, false),
                (typeof(YearBuiltDataController), nameof(YearBuiltDataController.GetCountByCountyIdAsync), "countyId", 1, false),
                (typeof(YearBuiltDataController), nameof(YearBuiltDataController.GetItemsByReferencesAsync), "countyId", 1, false)
            ];

            Assert.NotEmpty(parameters);

            foreach ((Type type, string method, string parameter, double minimum, bool exclusive) in parameters)
            {
                MinimumAttribute? minimumAttribute = FindMinimum(type, method, parameter);

                Assert.True(minimumAttribute is not null, $"Missing [Minimum] on {type.Name}.{method}({parameter}).");
                Assert.Equal(minimum, minimumAttribute!.Minimum);
                Assert.Equal(exclusive, minimumAttribute.Exclusive);
            }
        }

        /// <summary>
        /// Asserts the query parameters that look floored but are not stay without <see cref="MinimumAttribute"/>, so the attribute never claims a guard the action does not keep.
        /// <para>Named explicitly rather than derived, because the classification is a per-endpoint decision: an unguarded <c>commandTimeout</c> flows to the database as it is, and a selector whose unknown key is answered by a lookup rather than by a domain check - an id of 0 is a lookup miss, not an out-of-domain value - must stay unbounded in the document (ZiolkowskiJakub/DiGi.GIS.WebAPI#47).</para>
        /// </summary>
        [Fact]
        public void GisControllers_UnflooredQueryParameters_DoNotCarryMinimum()
        {
            (Type Type, string Method, string Parameter)[] parameters =
            [
                // commandTimeout with no action guard - a floor here would be a behaviour change, not a description of one.
                (typeof(BuildingDataController), nameof(BuildingDataController.UpdateItemsByCountyIdsAsync), "commandTimeout"),
                (typeof(OrtoDatasController), nameof(OrtoDatasController.GetEstimatedCoverageFactorAsync), "commandTimeout"),
                (typeof(OrtoDatasController), nameof(OrtoDatasController.GetSummariesByCountyIdsAsync), "commandTimeout"),
                (typeof(OrtoDatasController), nameof(OrtoDatasController.GetSubdivisionLinksByCountyIdAsync), "commandTimeout"),
                (typeof(OrtoDatasController), nameof(OrtoDatasController.GetQueueSummariesByCountyIdsAsync), "commandTimeout"),
                (typeof(OrtoDatasController), nameof(OrtoDatasController.NextBuilding2DReferencesAsync), "commandTimeout"),
                (typeof(TerrainController), nameof(TerrainController.GetSummariesByCountyIdsAsync), "commandTimeout"),
                (typeof(TerrainController), nameof(TerrainController.GetDensitiesByCountyIdsAsync), "commandTimeout"),
                (typeof(TerrainController), nameof(TerrainController.GetCoverageByCountyIdAsync), "commandTimeout"),
                (typeof(TerrainController), nameof(TerrainController.GetGapsByBoundingBoxAsync), "commandTimeout"),

                // Selectors answered by lookup, not by domain: an unknown or non-positive key is a miss (404 or empty) or a converter null check, not a range refusal.
                (typeof(BuildingController), nameof(BuildingController.GetCountAsync), "countyId"),
                (typeof(BuildingDataController), nameof(BuildingDataController.GetCountByCountyIdAsync), "countyId"),
                (typeof(BuildingDataController), nameof(BuildingDataController.GetTableByReferenceAsync), "countyId"),
                (typeof(BuildingDataController), nameof(BuildingDataController.GetUniqueValuesAsync), "countyId"),
                (typeof(BuildingModelController), nameof(BuildingModelController.GetCountByCountyIdAsync), "countyId"),
                (typeof(TerrainController), nameof(TerrainController.GetCountByCountyIdAsync), "countyId"),
                (typeof(TerrainController), nameof(TerrainController.GetCoverageByCountyIdAsync), "countyId"),
                (typeof(OccupancyDataController), nameof(OccupancyDataController.GetBuilding2DDuplicateReferencesAsync), "countyId"),
                (typeof(OccupancyDataController), nameof(OccupancyDataController.GetBuilding2DDuplicatesCountAsync), "countyId"),
                (typeof(YearBuiltDataController), nameof(YearBuiltDataController.GetItemsByReferenceAsync), "countyId"),
                (typeof(YearBuiltDataController), nameof(YearBuiltDataController.GetReferenceDuplicatesAsync), "countyId"),
                (typeof(OrtoDatasController), nameof(OrtoDatasController.GetCountByCountyIdAsync), "countyId"),
                (typeof(OrtoDatasController), nameof(OrtoDatasController.GetSubdivisionLinksByCountyIdAsync), "countyId"),
                (typeof(OrtoDatasController), nameof(OrtoDatasController.GetEstimatedCoverageFactorAsync), "administrativeAreal2DId"),
                (typeof(Building2DController), nameof(Building2DController.GetBuilding2DReferencesByAdministrativeAreal2DIdAsync), "administrativeAreal2DId"),
                (typeof(Building2DController), nameof(Building2DController.GetItemByIdAsync), "id")
            ];

            Assert.NotEmpty(parameters);

            foreach ((Type type, string method, string parameter) in parameters)
            {
                MinimumAttribute? minimumAttribute = FindMinimum(type, method, parameter);

                Assert.True(minimumAttribute is null, $"Unexpected [Minimum] on {type.Name}.{method}({parameter}): that action does not guard the parameter's domain.");
            }
        }

        /// <summary>
        /// Asserts the subdivision comparison's sample count carries both of its bounds - the one parameter with a real ceiling - through the built-in <see cref="RangeAttribute"/>, which Swashbuckle maps without a filter.
        /// </summary>
        [Fact]
        public void OrtoDatasController_SampleCount_CarriesRange()
        {
            RangeAttribute? rangeAttribute_SampleCount = FindRange(typeof(OrtoDatasController), nameof(OrtoDatasController.GetSubdivisionLinksByCountyIdAsync), "sampleCount");
            Assert.NotNull(rangeAttribute_SampleCount);
            Assert.Equal(0, System.Convert.ToDouble(rangeAttribute_SampleCount!.Minimum));
            Assert.Equal(Constants.OrtoDatas.MaximumSampleCount, System.Convert.ToDouble(rangeAttribute_SampleCount.Maximum));
        }

        /// <summary>
        /// Tests <see cref="MinimumAttribute"/> at its boundaries: the floor itself passes inclusively (or fails exclusively), a value just below fails, null passes, and <see cref="double.NaN"/> fails.
        /// <para>Constructed as a failing guard rather than a passing range: each invalid case was first observed to return a <see cref="ValidationResult"/> before the valid cases were asserted beside it, so the assertion cannot be decoration (ZiolkowskiJakub/DiGi.GIS.WebAPI#47).</para>
        /// </summary>
        [Fact]
        public void MinimumAttribute_Boundaries()
        {
            ValidationContext validationContext = new(new object());

            MinimumAttribute minimumAttribute_Zero = new(0);
            Assert.Null(minimumAttribute_Zero.GetValidationResult(0, validationContext));
            Assert.Null(minimumAttribute_Zero.GetValidationResult(1, validationContext));
            Assert.NotNull(minimumAttribute_Zero.GetValidationResult(-1, validationContext));
            Assert.NotNull(minimumAttribute_Zero.GetValidationResult(double.NaN, validationContext));

            MinimumAttribute minimumAttribute_One = new(1);
            Assert.Null(minimumAttribute_One.GetValidationResult(1, validationContext));
            Assert.Null(minimumAttribute_One.GetValidationResult(2, validationContext));
            Assert.NotNull(minimumAttribute_One.GetValidationResult(0, validationContext));

            MinimumAttribute minimumAttribute_Exclusive = new(0) { Exclusive = true };
            Assert.Null(minimumAttribute_Exclusive.GetValidationResult(0.001, validationContext));
            Assert.NotNull(minimumAttribute_Exclusive.GetValidationResult(0, validationContext));

            // Null is an omitted optional parameter: a mandatory one is refused by [BindRequired], not by the floor.
            Assert.Null(minimumAttribute_Zero.GetValidationResult(null, validationContext));
            Assert.Null(minimumAttribute_One.GetValidationResult(null, validationContext));
            Assert.Null(minimumAttribute_Exclusive.GetValidationResult(null, validationContext));
        }

        private static MinimumAttribute? FindMinimum(Type type, string method, string parameter)
        {
            return FindQueryParameter(type, method, parameter).GetCustomAttribute<MinimumAttribute>();
        }
    }
}
