using DiGi.GIS.PostgreSQL.Classes;
using Npgsql;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that a referenced object mismatch measurement carries every field through the constructor, a JSON round trip and a clone.
        /// <para>These figures are the before and after of a part repair, read over HTTP from a machine that has no access to the database, so a field lost in serialization is a figure nobody can check.</para>
        /// </summary>
        [Fact]
        public void Building2DReferencedObjectCountyPartMismatchResult_Serialization()
        {
            Building2DReferencedObjectCountyPartMismatchResult building2DReferencedObjectCountyPartMismatchResult = new("3020", 97360, [97358, 97360, 97364], 37_260, 30_000, 7_260);

            Assert.Equal("3020", building2DReferencedObjectCountyPartMismatchResult.Code);
            Assert.Equal(97360, building2DReferencedObjectCountyPartMismatchResult.CountyId);
            Assert.Equal(37_260, building2DReferencedObjectCountyPartMismatchResult.Count);
            Assert.Equal(30_000, building2DReferencedObjectCountyPartMismatchResult.CountHeldElsewhere);
            Assert.Equal(7_260, building2DReferencedObjectCountyPartMismatchResult.CountOrphan);
            Assert.NotNull(building2DReferencedObjectCountyPartMismatchResult.CountyIds);
            Assert.Equal(3, building2DReferencedObjectCountyPartMismatchResult.CountyIds.Count);

            string? json = Core.Convert.ToSystem_String(building2DReferencedObjectCountyPartMismatchResult);
            Assert.NotNull(json);

            Building2DReferencedObjectCountyPartMismatchResult? building2DReferencedObjectCountyPartMismatchResult_Json = Core.Convert.ToDiGi<Building2DReferencedObjectCountyPartMismatchResult>(json)?.FirstOrDefault();
            Assert.NotNull(building2DReferencedObjectCountyPartMismatchResult_Json);

            Assert.Equal("3020", building2DReferencedObjectCountyPartMismatchResult_Json.Code);
            Assert.Equal(97360, building2DReferencedObjectCountyPartMismatchResult_Json.CountyId);
            Assert.Equal(37_260, building2DReferencedObjectCountyPartMismatchResult_Json.Count);
            Assert.Equal(30_000, building2DReferencedObjectCountyPartMismatchResult_Json.CountHeldElsewhere);
            Assert.Equal(7_260, building2DReferencedObjectCountyPartMismatchResult_Json.CountOrphan);
            Assert.NotNull(building2DReferencedObjectCountyPartMismatchResult_Json.CountyIds);
            Assert.Contains(97358, building2DReferencedObjectCountyPartMismatchResult_Json.CountyIds);

            Building2DReferencedObjectCountyPartMismatchResult building2DReferencedObjectCountyPartMismatchResult_Clone = new(building2DReferencedObjectCountyPartMismatchResult);

            Assert.Equal("3020", building2DReferencedObjectCountyPartMismatchResult_Clone.Code);
            Assert.Equal(97360, building2DReferencedObjectCountyPartMismatchResult_Clone.CountyId);
            Assert.Equal(37_260, building2DReferencedObjectCountyPartMismatchResult_Clone.Count);
            Assert.Equal(30_000, building2DReferencedObjectCountyPartMismatchResult_Clone.CountHeldElsewhere);
            Assert.Equal(7_260, building2DReferencedObjectCountyPartMismatchResult_Clone.CountOrphan);
            Assert.NotNull(building2DReferencedObjectCountyPartMismatchResult_Clone.CountyIds);
            Assert.Equal(3, building2DReferencedObjectCountyPartMismatchResult_Clone.CountyIds.Count);

            Core.xUnit.Query.SerializationCheck(building2DReferencedObjectCountyPartMismatchResult);
        }

        /// <summary>
        /// Verifies that a part holding only orphan rows reports nothing held elsewhere, that a part holding only misplaced rows reports no orphans, and that the split always adds up to the total.
        /// </summary>
        [Fact]
        public void Building2DReferencedObjectCountyPartMismatchResult_Split()
        {
            Building2DReferencedObjectCountyPartMismatchResult building2DReferencedObjectCountyPartMismatchResult_OrphanOnly = new("2405", 76984, [76984, 76989], 10, 0, 10);

            Assert.Equal(0, building2DReferencedObjectCountyPartMismatchResult_OrphanOnly.CountHeldElsewhere);
            Assert.Equal(10, building2DReferencedObjectCountyPartMismatchResult_OrphanOnly.CountOrphan);
            Assert.Equal(building2DReferencedObjectCountyPartMismatchResult_OrphanOnly.CountHeldElsewhere + building2DReferencedObjectCountyPartMismatchResult_OrphanOnly.CountOrphan, building2DReferencedObjectCountyPartMismatchResult_OrphanOnly.Count);

            Building2DReferencedObjectCountyPartMismatchResult building2DReferencedObjectCountyPartMismatchResult_HeldElsewhereOnly = new("2405", 76984, [76984, 76989], 10, 10, 0);

            Assert.Equal(0, building2DReferencedObjectCountyPartMismatchResult_HeldElsewhereOnly.CountOrphan);
            Assert.Equal(10, building2DReferencedObjectCountyPartMismatchResult_HeldElsewhereOnly.CountHeldElsewhere);
            Assert.Equal(building2DReferencedObjectCountyPartMismatchResult_HeldElsewhereOnly.CountHeldElsewhere + building2DReferencedObjectCountyPartMismatchResult_HeldElsewhereOnly.CountOrphan, building2DReferencedObjectCountyPartMismatchResult_HeldElsewhereOnly.Count);

            Core.xUnit.Query.SerializationCheck(building2DReferencedObjectCountyPartMismatchResult_OrphanOnly);
            Core.xUnit.Query.SerializationCheck(building2DReferencedObjectCountyPartMismatchResult_HeldElsewhereOnly);
        }

        /// <summary>
        /// Verifies that <see cref="Building2DReferencedObjectPostgreSQLConverter{T1, T2}.GetCountyPartMismatchesAsync(NpgsqlConnection, string, int, System.Threading.CancellationToken)"/>
        /// and its parameterless overload return null when given invalid parameters or null connections.
        /// </summary>
        [Fact]
        public async Task GetCountyPartMismatchesAsync_Guards()
        {
            YearBuiltDataPostgreSQLConverter yearBuiltDataPostgreSQLConverter = new(null);

            List<Building2DReferencedObjectCountyPartMismatchResult>? mismatches_NullConnection = await yearBuiltDataPostgreSQLConverter.GetCountyPartMismatchesAsync((NpgsqlConnection?)null);
            Assert.Null(mismatches_NullConnection);

            List<Building2DReferencedObjectCountyPartMismatchResult>? mismatches_NullConnectionData = await yearBuiltDataPostgreSQLConverter.GetCountyPartMismatchesAsync(code: null);
            Assert.Null(mismatches_NullConnectionData);

            List<Building2DReferencedObjectCountyPartMismatchResult>? mismatches_NegativeTimeout = await yearBuiltDataPostgreSQLConverter.GetCountyPartMismatchesAsync(commandTimeout: -1);
            Assert.Null(mismatches_NegativeTimeout);
        }

        /// <summary>
        /// Verifies that <see cref="Building2DReferencedObjectPostgreSQLConverter{T1, T2}.GetCountyPartMismatchesAsync(string, int, System.Threading.CancellationToken)"/>
        /// executes successfully against a live database with explicit command timeout.
        /// <para>Skipped by default: it executes an integration query requiring <c>GIS_PostgreSQL_Storage.conf</c> pointing at a database.</para>
        /// </summary>
        [Fact(Skip = "Executes an integration query. Point GIS_PostgreSQL_Storage.conf at a database before running.")]
        public async Task GetCountyPartMismatchesAsync_Integration()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            YearBuiltDataPostgreSQLConverter? yearBuiltDataPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<YearBuiltDataPostgreSQLConverter>();
            Assert.NotNull(yearBuiltDataPostgreSQLConverter);

            List<Building2DReferencedObjectCountyPartMismatchResult>? mismatches = await yearBuiltDataPostgreSQLConverter.GetCountyPartMismatchesAsync(code: null, commandTimeout: 600);
            Assert.NotNull(mismatches);
        }
    }
}
