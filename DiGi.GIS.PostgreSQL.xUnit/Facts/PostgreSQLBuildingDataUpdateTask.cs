using DiGi.GIS.PostgreSQL.Classes;
using DiGi.GIS.PostgreSQL.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="PostgreSQLBuildingDataUpdateTask"/> initializes its default properties and validates null arguments.
        /// </summary>
        [Fact]
        public void PostgreSQLBuildingDataUpdateTask_Constructor()
        {
            Assert.Throws<ArgumentNullException>(() => new PostgreSQLBuildingDataUpdateTask(null!));

            GISPostgreSQLConverterManager gISPostgreSQLConverterManager = new();
            PostgreSQLBuildingDataUpdateTask postgreSQLBuildingDataUpdateTask = new(gISPostgreSQLConverterManager);

            Assert.NotNull(postgreSQLBuildingDataUpdateTask.PostgreSQLBuildingDataUpdateOptions);
            Assert.Equal(0, postgreSQLBuildingDataUpdateTask.FailedSubdivisionCount);
            Assert.Equal(0, postgreSQLBuildingDataUpdateTask.ProcessedSubdivisionCount);
            Assert.Equal(0, postgreSQLBuildingDataUpdateTask.SkippedSubdivisionCount);
            Assert.Equal(0, postgreSQLBuildingDataUpdateTask.UnassignedSubdivisionBuildingCount);
            Assert.Equal(0, postgreSQLBuildingDataUpdateTask.CrossCountySubdivisionBuildingCount);
            Assert.Equal(0, postgreSQLBuildingDataUpdateTask.UpdatedRowCount);
        }

        /// <summary>
        /// Verifies that <see cref="PostgreSQLBuildingDataUpdateTask"/> correctly processes buildings for multi-part counties where subdivisions and buildings reside on different sibling county parts.
        /// <para>Skipped by default: requires PostgreSQL configuration files pointing at a database populated with administrative areal and building data.</para>
        /// </summary>
        [Fact(Skip = "Requires the PostgreSQL configuration files pointing at a database.")]
        public async Task PostgreSQLBuildingDataUpdateTask_MultiPartCounty_Integration()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            BuildingDataPostgreSQLConverter? buildingDataPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<BuildingDataPostgreSQLConverter>();
            Assert.NotNull(buildingDataPostgreSQLConverter);

            Building2DPostgreSQLConverter? building2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DPostgreSQLConverter>();
            Assert.NotNull(building2DPostgreSQLConverter);

            // Test county: czestochowski (TERYT 2404, building partition 76453, subdivision parent 76454)
            int countyId = 76453;

            PostgreSQLBuildingDataUpdateTask postgreSQLBuildingDataUpdateTask = new(gISPostgreSQLConverterManager)
            {
                PostgreSQLBuildingDataUpdateOptions = new PostgreSQLBuildingDataUpdateOptions
                {
                    BuildingDataUpdateTypes = [BuildingDataUpdateType.General, BuildingDataUpdateType.Database],
                    CountyIds = [countyId]
                }
            };

            TaskCompletionSource<bool> taskCompletionSource = new();
            postgreSQLBuildingDataUpdateTask.Stopped += (object? sender, EventArgs e) => taskCompletionSource.TrySetResult(true);

            postgreSQLBuildingDataUpdateTask.Start();

            await taskCompletionSource.Task;

            Assert.Null(postgreSQLBuildingDataUpdateTask.Exception);
            Assert.True(postgreSQLBuildingDataUpdateTask.IsSucceeded);
            Assert.Equal(0, postgreSQLBuildingDataUpdateTask.FailedSubdivisionCount);

            BuildingDataCoverageResult? buildingDataCoverageResult = await buildingDataPostgreSQLConverter.BuildingDataCoverageResultAsync(building2DPostgreSQLConverter, countyId);
            Assert.NotNull(buildingDataCoverageResult);
            Assert.Equal(0, buildingDataCoverageResult.MissingReferenceCount);
        }

        /// <summary>
        /// Verifies that a subdivision filed under one county part is in scope only for the parts of that part's code group, never for an unrelated part.
        /// <para>The figures are the live data of the defect: subdivision 80392 (Baranowice) is filed under part 80379 (Miasto Żory), so the buildings it carries in part 77971 are invisible to the subdivision loop and only the fallback can write them.</para>
        /// </summary>
        [Fact]
        public void InScopeSubdivisionIds_CrossCountySubdivision()
        {
            List<AdministrativeAreal2DReference> subdivisions =
            [
                new()
                {
                    Id = 80392,
                    CountyId = 80379,
                    Code = "2479011"
                }
            ];

            Dictionary<int, HashSet<int>> siblingCountyGroups = new()
            {
                [77971] = [77971],
                [80379] = [80379]
            };

            Dictionary<int, HashSet<int>> inScopeSubdivisionIds = Query.InScopeSubdivisionIds(subdivisions, siblingCountyGroups);

            Assert.Contains(80379, inScopeSubdivisionIds);
            Assert.Contains(80392, inScopeSubdivisionIds[80379]);

            // The defect case: 80392 must not be treated as in scope under 77971, or the fallback would re-process what the loop already wrote.
            Assert.DoesNotContain(77971, inScopeSubdivisionIds);
        }

        /// <summary>
        /// Verifies that a subdivision filed under one part of a multi-part county is in scope for every part of the code group, because the subdivision loop visits all of them.
        /// </summary>
        [Fact]
        public void InScopeSubdivisionIds_SiblingPartsShareScope()
        {
            List<AdministrativeAreal2DReference> subdivisions =
            [
                new()
                {
                    Id = 50000,
                    CountyId = 73482
                }
            ];

            Dictionary<int, HashSet<int>> siblingCountyGroups = new()
            {
                [73482] = [73482, 73485],
                [73485] = [73482, 73485]
            };

            Dictionary<int, HashSet<int>> inScopeSubdivisionIds = Query.InScopeSubdivisionIds(subdivisions, siblingCountyGroups);

            Assert.Contains(50000, inScopeSubdivisionIds[73482]);
            Assert.Contains(50000, inScopeSubdivisionIds[73485]);
        }

        /// <summary>
        /// Verifies that the instance and static overloads of <c>Building2DPostgreSQLConverter.GetBuilding2DsUnreachedByCountyAsync</c> return null over a null connection.
        /// </summary>
        [Fact]
        public async Task GetBuilding2DsUnreachedByCountyAsync_NullConnection_ReturnsNull()
        {
            List<Building2D>? buildings_StaticNullConnection = await Building2DPostgreSQLConverter.GetBuilding2DsUnreachedByCountyAsync(null, 77971, [80392]);
            Assert.Null(buildings_StaticNullConnection);

            Building2DPostgreSQLConverter building2DPostgreSQLConverter = new(null);

            List<Building2D>? buildings_InstanceNullConnectionData = await building2DPostgreSQLConverter.GetBuilding2DsUnreachedByCountyAsync(77971, [80392]);
            Assert.Null(buildings_InstanceNullConnectionData);
        }

        /// <summary>
        /// Verifies that cross-county buildings - filed under a county part but subdivided under a neighbouring county - get a building data row after a run.
        /// <para>County part 77971 carries two such buildings under subdivision 80392 (filed under neighbouring part 80379), which the subdivision loop cannot reach. Before the fix the fallback read only <c>subdivision_id IS NULL</c> buildings, so both were left out and the coverage reported two missing references; after the fix the set difference is empty.</para>
        /// <para>Skipped by default: requires PostgreSQL configuration files pointing at a database.</para>
        /// </summary>
        [Fact(Skip = "Requires the PostgreSQL configuration files pointing at a database.")]
        public async Task PostgreSQLBuildingDataUpdateTask_CrossCountySubdivision_Integration()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            BuildingDataPostgreSQLConverter? buildingDataPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<BuildingDataPostgreSQLConverter>();
            Assert.NotNull(buildingDataPostgreSQLConverter);

            Building2DPostgreSQLConverter? building2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DPostgreSQLConverter>();
            Assert.NotNull(building2DPostgreSQLConverter);

            // Test county: part 77971, whose two cross-county buildings sit under subdivision 80392 (parent part 80379).
            int countyId = 77971;

            PostgreSQLBuildingDataUpdateTask postgreSQLBuildingDataUpdateTask = new(gISPostgreSQLConverterManager)
            {
                PostgreSQLBuildingDataUpdateOptions = new PostgreSQLBuildingDataUpdateOptions
                {
                    BuildingDataUpdateTypes = [BuildingDataUpdateType.General, BuildingDataUpdateType.Database],
                    CountyIds = [countyId]
                }
            };

            TaskCompletionSource<bool> taskCompletionSource = new();
            postgreSQLBuildingDataUpdateTask.Stopped += (object? sender, EventArgs e) => taskCompletionSource.TrySetResult(true);

            postgreSQLBuildingDataUpdateTask.Start();

            await taskCompletionSource.Task;

            Assert.Null(postgreSQLBuildingDataUpdateTask.Exception);
            Assert.True(postgreSQLBuildingDataUpdateTask.IsSucceeded);
            Assert.Equal(0, postgreSQLBuildingDataUpdateTask.FailedSubdivisionCount);
            Assert.True(postgreSQLBuildingDataUpdateTask.CrossCountySubdivisionBuildingCount > 0);

            BuildingDataCoverageResult? buildingDataCoverageResult = await buildingDataPostgreSQLConverter.BuildingDataCoverageResultAsync(building2DPostgreSQLConverter, countyId);
            Assert.NotNull(buildingDataCoverageResult);
            Assert.Equal(0, buildingDataCoverageResult.MissingReferenceCount);
        }

        /// <summary>
        /// Verifies that the per-county fallback set never contains a building the subdivision loop reaches: for part 77971, the buildings under an in-scope subdivision and the buildings returned by <see cref="Building2DPostgreSQLConverter.GetBuilding2DsUnreachedByCountyAsync(int, IEnumerable{int}, int, System.Threading.CancellationToken)" /> are disjoint.
        /// <para>The in-scope set is built exactly the way <see cref="PostgreSQLBuildingDataUpdateTask" /> builds it - <see cref="Query.SiblingCountyGroups(IEnumerable{AdministrativeAreal2DReference}?)" /> and <see cref="Query.InScopeSubdivisionIds(IEnumerable{AdministrativeAreal2DReference}?, IReadOnlyDictionary{int, HashSet{int}}?)" /> - so this asserts the caller-side invariant the fallback's safety relies on: the <c>Update_Building2D</c> upsert it pushes never clobbers a cell the loop has already written.</para>
        /// <para>Part 77971 (Miasto Żory) carries buildings under in-scope subdivisions and two cross-county buildings under subdivision 80392 (parent part 80379), so both sets are non-empty and the disjointness is meaningful.</para>
        /// <para>Skipped by default: requires PostgreSQL configuration files pointing at a database populated with administrative areal and building data.</para>
        /// </summary>
        [Fact(Skip = "Requires the PostgreSQL configuration files pointing at a database.")]
        public async Task PostgreSQLBuildingDataUpdateTask_FallbackExcludesInScopeBuildings_Integration()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<AdministrativeAreal2DPostgreSQLConverter>();
            Assert.NotNull(administrativeAreal2DPostgreSQLConverter);

            Building2DPostgreSQLConverter? building2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DPostgreSQLConverter>();
            Assert.NotNull(building2DPostgreSQLConverter);

            // Test county: part 77971, whose two cross-county buildings sit under subdivision 80392 (parent part 80379).
            int countyId = 77971;

            List<AdministrativeAreal2DReference>? subdivisions = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(AdministrativeArealType.Subdivision);
            Assert.NotNull(subdivisions);
            Assert.True(subdivisions.Count > 0, "The administrative areal data is expected to carry subdivisions.");

            List<AdministrativeAreal2DReference>? countyReferences = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(AdministrativeArealType.County);
            Assert.NotNull(countyReferences);
            Assert.True(countyReferences.Count > 0, "The administrative areal data is expected to carry counties.");

            Dictionary<int, HashSet<int>> siblingCountyGroups = countyReferences.SiblingCountyGroups();

            Dictionary<int, HashSet<int>> inScopeSubdivisionIds_ByCountyId = Query.InScopeSubdivisionIds(subdivisions, siblingCountyGroups);

            Assert.True(inScopeSubdivisionIds_ByCountyId.TryGetValue(countyId, out HashSet<int>? inScopeSubdivisionIds) && inScopeSubdivisionIds.Count > 0, $"Part {countyId} is expected to carry in-scope subdivisions, or the fact is vacuous.");

            List<Building2D>? buildings_InScope = await building2DPostgreSQLConverter.GetBuilding2DsByCountyIdAsync(countyId, inScopeSubdivisionIds);
            Assert.NotNull(buildings_InScope);

            // The read also returns the unassigned buildings of the part (subdivision_id IS NULL), which belong to the fallback by design - keep only the buildings under an in-scope subdivision.
            HashSet<long> inScopeBuildingIds = [];
            foreach (Building2D building2D in buildings_InScope)
            {
                if (building2D.SubdivisionId is int subdivisionId && inScopeSubdivisionIds.Contains(subdivisionId))
                {
                    inScopeBuildingIds.Add(building2D.Id);
                }
            }

            Assert.True(inScopeBuildingIds.Count > 0, $"Part {countyId} is expected to carry buildings under an in-scope subdivision, or the fact is vacuous.");

            List<Building2D>? buildings_Fallback = await building2DPostgreSQLConverter.GetBuilding2DsUnreachedByCountyAsync(countyId, inScopeSubdivisionIds);
            Assert.NotNull(buildings_Fallback);
            Assert.True(buildings_Fallback.Count > 0, $"Part {countyId} is expected to carry fallback buildings (unassigned or cross-county), or the fact is vacuous.");

            // The invariant: a building the subdivision loop reaches is never handed to the fallback, so the upsert the fallback pushes never clobbers a cell the loop has already written.
            foreach (Building2D building2D in buildings_Fallback)
            {
                Assert.False(inScopeBuildingIds.Contains(building2D.Id), $"Fallback building {building2D.Id} (reference {building2D.Reference}) sits under an in-scope subdivision - the fallback would re-process what the loop already wrote.");
            }
        }

        /// <summary>
        /// Verifies that <c>Query.Match</c> over a subdivision reference and its path prefers the subdivision's own match and falls back to the path's municipality reference only when the subdivision does not match.
        /// <para>This is the resolution both passes of <see cref="PostgreSQLBuildingDataUpdateTask"/> share: the subdivision loop for its subdivisions, and the unassigned pass for the cross-county buildings' own subdivisions.</para>
        /// </summary>
        [Fact]
        public void Match_SubdivisionReference_MunicipalityFallback()
        {
            List<DiGi.BDL.Classes.Unit> units =
            [
                new DiGi.BDL.Classes.Unit { id = "000000000000", name = "POLSKA", level = 0 },
                new DiGi.BDL.Classes.Unit { id = "030000000000", name = "MAKROREGION POŁUDNIOWO-ZACHODNI", level = 1 },
                new DiGi.BDL.Classes.Unit { id = "030200000000", name = "DOLNOŚLĄSKIE", level = 2 },
                new DiGi.BDL.Classes.Unit { id = "030210000000", name = "REGION DOLNOŚLĄSKIE", level = 3 },
                new DiGi.BDL.Classes.Unit { id = "030210100000", name = "PODREGION JELENIOGÓRSKI", level = 4 },
                new DiGi.BDL.Classes.Unit { id = "030210101000", name = "POWIAT BOLESŁAWIECKI", level = 5 },
                new DiGi.BDL.Classes.Unit { id = "030210101011", name = "BOLESŁAWIEC", level = 6 },
                new DiGi.BDL.Classes.Unit { id = "030210101012", name = "GROMADKA", level = 6 }
            ];

            DiGi.GIS.Classes.StatisticalUnit? rootStatisticalUnit = GIS.Create.StatisticalUnit(units);
            Assert.NotNull(rootStatisticalUnit);

            AdministrativeAreal2DReference municipalityReference = new()
            {
                Id = 901,
                Code = "0201011",
                Name = "BOLESŁAWIEC (GM. MIEJSKA)",
                AdministrativeArealType = AdministrativeArealType.Municipality
            };

            AdministrativeAreal2DReferencePath administrativeAreal2DReferencePath = new([municipalityReference]);
            Assert.NotNull(administrativeAreal2DReferencePath[AdministrativeArealType.Municipality]);

            // A subdivision that names its own statistical unit matches directly, even though the path's municipality is a different unit.
            AdministrativeAreal2DReference subdivisionReference_Matched = new()
            {
                Id = 900,
                Name = "GROMADKA",
                AdministrativeArealType = AdministrativeArealType.Subdivision
            };

            DiGi.GIS.Classes.StatisticalUnit? statisticalUnit_Subdivision = Query.Match(rootStatisticalUnit, subdivisionReference_Matched, administrativeAreal2DReferencePath);
            Assert.NotNull(statisticalUnit_Subdivision);
            Assert.Equal("GROMADKA", statisticalUnit_Subdivision.Name);

            // A subdivision that matches nothing falls back to the municipality of its path.
            AdministrativeAreal2DReference subdivisionReference_Unmatched = new()
            {
                Id = 902,
                Name = "OSIEDLE PÓŁNOC",
                AdministrativeArealType = AdministrativeArealType.Subdivision
            };

            DiGi.GIS.Classes.StatisticalUnit? statisticalUnit_Municipality = Query.Match(rootStatisticalUnit, subdivisionReference_Unmatched, administrativeAreal2DReferencePath);
            Assert.NotNull(statisticalUnit_Municipality);
            Assert.Equal("BOLESŁAWIEC", statisticalUnit_Municipality.Name);

            // Without a path there is nothing to fall back to.
            Assert.Null(Query.Match(rootStatisticalUnit, subdivisionReference_Unmatched, null));

            // A path without a municipality leaves nothing to fall back to either.
            AdministrativeAreal2DReference countyReference = new()
            {
                Id = 903,
                Code = "0201",
                Name = "BOLESŁAWIECKI",
                AdministrativeArealType = AdministrativeArealType.County
            };

            AdministrativeAreal2DReferencePath administrativeAreal2DReferencePath_County = new([countyReference]);
            Assert.Null(Query.Match(rootStatisticalUnit, subdivisionReference_Unmatched, administrativeAreal2DReferencePath_County));
        }

        /// <summary>
        /// Verifies that the buildings only the unassigned pass can reach - buildings naming a subdivision filed under another county part - receive the population columns when the task runs with <see cref="BuildingDataUpdateType.Statistical"/>.
        /// <para>The county is discovered rather than hardcoded: the parts of the multi-part county codes are probed until one carries cross-county buildings, because the set the unassigned pass reaches changes with repairs and re-imports. Before the fix the final per-county pass had no statistical branch, so those buildings were never written the Municipality population columns however often the task ran with Statistical selected.</para>
        /// <para>Skipped by default: requires PostgreSQL configuration files pointing at a database populated with administrative areal, building and statistical data.</para>
        /// </summary>
        [Fact(Skip = "Requires the PostgreSQL configuration files pointing at a database populated with administrative areal, building and statistical data.")]
        public async Task PostgreSQLBuildingDataUpdateTask_Statistical_UnassignedBuildings_Integration()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            AdministrativeAreal2DPostgreSQLConverter? administrativeAreal2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<AdministrativeAreal2DPostgreSQLConverter>();
            Assert.NotNull(administrativeAreal2DPostgreSQLConverter);

            UnitPostgreSQLConverter? unitPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<UnitPostgreSQLConverter>();
            Assert.NotNull(unitPostgreSQLConverter);

            DiGi.GIS.Classes.StatisticalUnit? rootStatisticalUnit = await unitPostgreSQLConverter.GetStatisticalUnitAsync(commandTimeout: 600);
            Assert.NotNull(rootStatisticalUnit);

            Building2DPostgreSQLConverter? building2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DPostgreSQLConverter>();
            Assert.NotNull(building2DPostgreSQLConverter);

            BuildingDataPostgreSQLConverter? buildingDataPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<BuildingDataPostgreSQLConverter>();
            Assert.NotNull(buildingDataPostgreSQLConverter);

            List<AdministrativeAreal2DReference>? subdivisions = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(AdministrativeArealType.Subdivision, commandTimeout: 600);
            Assert.NotNull(subdivisions);

            List<AdministrativeAreal2DReference>? countyReferences = await administrativeAreal2DPostgreSQLConverter.GetAdministrativeAreal2DReferencesByAdministrativeArealTypeAsync(AdministrativeArealType.County, commandTimeout: 600);
            Assert.NotNull(countyReferences);

            Dictionary<int, HashSet<int>> siblingCountyGroups = countyReferences.SiblingCountyGroups();
            Dictionary<int, HashSet<int>> inScopeSubdivisionIds_ByCountyId = Query.InScopeSubdivisionIds(subdivisions, siblingCountyGroups);

            // Discover a county whose unassigned set carries at least one building naming a (cross-county) subdivision: cross-county
            // filing concentrates in the parts of the multi-part county codes, so those parts are probed first and the single-part
            // codes only behind them, and the set is built exactly the way the task builds it.
            List<int> candidateCountyIds =
            [
                .. siblingCountyGroups.Where(x => x.Value.Count > 1).Select(x => x.Key),
                .. siblingCountyGroups.Where(x => x.Value.Count == 1).Select(x => x.Key)
            ];

            int countyId = -1;
            List<Building2D> building2Ds_Unassigned = [];
            foreach (int countyId_Candidate in candidateCountyIds)
            {
                inScopeSubdivisionIds_ByCountyId.TryGetValue(countyId_Candidate, out HashSet<int>? inScopeSubdivisionIds_Candidate);

                List<Building2D>? building2Ds_Candidate = await building2DPostgreSQLConverter.GetBuilding2DsUnreachedByCountyAsync(countyId_Candidate, inScopeSubdivisionIds_Candidate, commandTimeout: 600);
                if (building2Ds_Candidate is not null && building2Ds_Candidate.Any(x => x.SubdivisionId is not null))
                {
                    countyId = countyId_Candidate;
                    building2Ds_Unassigned = building2Ds_Candidate;
                    break;
                }
            }

            Assert.True(countyId != -1, "No county part of a multi-part code carries cross-county buildings on this database, or the fact is vacuous.");

            List<string> references_CrossCounty = [.. building2Ds_Unassigned.Where(x => x.SubdivisionId is not null && !string.IsNullOrWhiteSpace(x.Reference)).Select(x => x.Reference!)];
            Assert.True(references_CrossCounty.Count > 0, $"Part {countyId} is expected to carry cross-county buildings, or the fact is vacuous.");

            PostgreSQLBuildingDataUpdateTask postgreSQLBuildingDataUpdateTask = new(gISPostgreSQLConverterManager)
            {
                PostgreSQLBuildingDataUpdateOptions = new PostgreSQLBuildingDataUpdateOptions
                {
                    BuildingDataUpdateTypes = [BuildingDataUpdateType.Statistical],
                    CountyIds = [countyId]
                }
            };

            TaskCompletionSource<bool> taskCompletionSource = new();
            postgreSQLBuildingDataUpdateTask.Stopped += (object? sender, EventArgs e) => taskCompletionSource.TrySetResult(true);

            postgreSQLBuildingDataUpdateTask.Start();

            await taskCompletionSource.Task;

            Assert.Null(postgreSQLBuildingDataUpdateTask.Exception);
            Assert.True(postgreSQLBuildingDataUpdateTask.IsSucceeded);
            Assert.Equal(0, postgreSQLBuildingDataUpdateTask.FailedSubdivisionCount);
            Assert.True(postgreSQLBuildingDataUpdateTask.CrossCountySubdivisionBuildingCount > 0, "The unassigned pass is expected to have processed the cross-county buildings.");

            Core.IO.Table.Classes.Table? table = await buildingDataPostgreSQLConverter.PullAsync(references_CrossCounty, countyId, commandTimeout: 600);
            Assert.NotNull(table);
            Assert.True(table.RowCount > 0, $"The building data pull of part {countyId} returned no rows for the cross-county references - the unassigned pass did not write them.");

            // The defect: those rows carried no Municipality population columns at all, so the column lookup itself failed on unmodified code.
            Assert.True(table.Columns.Any(x => x.Name?.StartsWith("Municipality population ") == true), $"The pulled building data of part {countyId} carries no Municipality population column - the unassigned pass did not write the population columns.");

            foreach (Core.IO.Table.Classes.Row row in table.Rows)
            {
                bool populationFound = false;
                foreach (Core.IO.Table.Classes.Column column in table.Columns)
                {
                    if (column.Name?.StartsWith("Municipality population ") == true && row.TryGetValue(column.Index, out int population) && population > 0)
                    {
                        populationFound = true;
                        break;
                    }
                }

                Assert.True(populationFound, $"Row {row.Index} of the pulled building data carries no positive Municipality population value in any year.");
            }
        }
    }
}
