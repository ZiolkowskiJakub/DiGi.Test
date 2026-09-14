using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.PostgreSQL.Classes;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiGi.GIS.PostgreSQL.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Building2DCentroid"/> carries every field - reference, county partition and the centroid coordinates - through the constructor, a JSON round trip and a clone.
        /// <para>The reference and the county partition travel together because a reference is unique only in combination with the county part it was imported under. A coordinate lost in serialization is a dot rendered at the wrong place on a map that has no way to know.</para>
        /// </summary>
        [Fact]
        public void Building2DCentroid_Serialization()
        {
            Building2DCentroid building2DCentroid = new()
            {
                Reference = "28A8E11F-6255-8A99-E053-CA2BA8C0EC21",
                CountyId = 73485,
                X = 467123.5,
                Y = 6721587.25,
            };

            Assert.Equal("28A8E11F-6255-8A99-E053-CA2BA8C0EC21", building2DCentroid.Reference);
            Assert.Equal(73485, building2DCentroid.CountyId);
            Assert.Equal(467123.5, building2DCentroid.X);
            Assert.Equal(6721587.25, building2DCentroid.Y);

            string? json = Core.Convert.ToSystem_String(building2DCentroid);
            Assert.NotNull(json);

            Building2DCentroid? building2DCentroid_Json = Core.Convert.ToDiGi<Building2DCentroid>(json)?.FirstOrDefault();
            Assert.NotNull(building2DCentroid_Json);

            Assert.Equal("28A8E11F-6255-8A99-E053-CA2BA8C0EC21", building2DCentroid_Json.Reference);
            Assert.Equal(73485, building2DCentroid_Json.CountyId);
            Assert.Equal(467123.5, building2DCentroid_Json.X);
            Assert.Equal(6721587.25, building2DCentroid_Json.Y);

            Building2DCentroid building2DCentroid_Clone = new(building2DCentroid);

            Assert.Equal("28A8E11F-6255-8A99-E053-CA2BA8C0EC21", building2DCentroid_Clone.Reference);
            Assert.Equal(73485, building2DCentroid_Clone.CountyId);
            Assert.Equal(467123.5, building2DCentroid_Clone.X);
            Assert.Equal(6721587.25, building2DCentroid_Clone.Y);

            Core.xUnit.Query.SerializationCheck(building2DCentroid);
        }

        /// <summary>
        /// Verifies that <see cref="Building2DPostgreSQLConverter.CentroidFromBoundingBox(double?, double?, double?, double?)"/> returns the centre of a known box.
        /// </summary>
        [Fact]
        public void CentroidFromBoundingBox_KnownBox_ReturnsCentre()
        {
            Point2D? point2D = Building2DPostgreSQLConverter.CentroidFromBoundingBox(10.0, 20.0, 30.0, 40.0);

            Assert.NotNull(point2D);
            Assert.Equal(20.0, point2D.X);
            Assert.Equal(30.0, point2D.Y);
        }

        /// <summary>
        /// Verifies that <see cref="Building2DPostgreSQLConverter.CentroidFromBoundingBox(double?, double?, double?, double?)"/> returns null when any single component of the bounding box is missing.
        /// <para>A NULL column on the wire means the row holds no usable box, so no centre may be returned for it - a caller reading null must skip the row, not guess.</para>
        /// </summary>
        [Fact]
        public void CentroidFromBoundingBox_MissingComponent_ReturnsNull()
        {
            Assert.Null(Building2DPostgreSQLConverter.CentroidFromBoundingBox(null, 20.0, 30.0, 40.0));
            Assert.Null(Building2DPostgreSQLConverter.CentroidFromBoundingBox(10.0, null, 30.0, 40.0));
            Assert.Null(Building2DPostgreSQLConverter.CentroidFromBoundingBox(10.0, 20.0, null, 40.0));
            Assert.Null(Building2DPostgreSQLConverter.CentroidFromBoundingBox(10.0, 20.0, 30.0, null));
        }

        /// <summary>
        /// Verifies that <see cref="Building2DPostgreSQLConverter.CentroidFromBoundingBox(double?, double?, double?, double?)"/> returns null when any single component of the bounding box is NaN.
        /// <para>The stored values are read as plain doubles, so a NaN on the wire must be treated as a missing box, exactly as the inline arithmetic of <c>GetPoint2DsByReferencesAsync</c> did before it was extracted.</para>
        /// </summary>
        [Fact]
        public void CentroidFromBoundingBox_NaNComponent_ReturnsNull()
        {
            double nan = double.NaN;

            Assert.Null(Building2DPostgreSQLConverter.CentroidFromBoundingBox(nan, 20.0, 30.0, 40.0));
            Assert.Null(Building2DPostgreSQLConverter.CentroidFromBoundingBox(10.0, nan, 30.0, 40.0));
            Assert.Null(Building2DPostgreSQLConverter.CentroidFromBoundingBox(10.0, 20.0, nan, 40.0));
            Assert.Null(Building2DPostgreSQLConverter.CentroidFromBoundingBox(10.0, 20.0, 30.0, nan));
        }

        /// <summary>
        /// Verifies that <see cref="Building2DPostgreSQLConverter.GetBuilding2DCentroidsByCountyIdAsync(Npgsql.NpgsqlConnection?, int, IEnumerable{int}?, int, System.Threading.CancellationToken)"/> returns null when given a null connection.
        /// </summary>
        [Fact]
        public async Task GetBuilding2DCentroidsByCountyIdAsync_NullConnection_ReturnsNull()
        {
            List<Building2DCentroid>? building2DCentroids = await Building2DPostgreSQLConverter.GetBuilding2DCentroidsByCountyIdAsync(null, 73485, [1, 2, 3]);
            Assert.Null(building2DCentroids);
        }

        /// <summary>
        /// Verifies that <see cref="Building2DPostgreSQLConverter.GetBuilding2DCentroidsByAdministrativeAreal2DIdsAsync(IEnumerable{int}, int, System.Threading.CancellationToken)"/> returns null when the converter has no connection data.
        /// </summary>
        [Fact]
        public async Task GetBuilding2DCentroidsByAdministrativeAreal2DIdsAsync_NullConnectionData_ReturnsNull()
        {
            Building2DPostgreSQLConverter building2DPostgreSQLConverter = new(null);

            List<Building2DCentroid>? building2DCentroids = await building2DPostgreSQLConverter.GetBuilding2DCentroidsByAdministrativeAreal2DIdsAsync([73485]);
            Assert.Null(building2DCentroids);
        }

        /// <summary>
        /// Verifies that <see cref="Building2DPostgreSQLConverter.GetBuilding2DCentroidsByAdministrativeAreal2DIdsAsync(IEnumerable{int}, int, System.Threading.CancellationToken)"/> answers with every item carrying a reference, a county partition and finite coordinates, and that its (reference, county) set is contained in the one answered by <c>GetBuilding2DReferencesByAdministrativeAreal2DIdsAsync</c> for the same areas - the centroid list may only be shorter, because rows without a bounding box are skipped.
        /// <para>The areas are resolved through Subdivision children, so an area the database does not hold answers empty on both sides, and the fact stays green - non-emptiness is a live-estate question, verified through the deployed API, not here.</para>
        /// <para>Skipped by default: requires a live, populated PostgreSQL database.</para>
        /// </summary>
        [Fact(Skip = "Executes an integration query. Point GIS_PostgreSQL_Main.conf at a database before running.")]
        public async Task GetBuilding2DCentroidsByAdministrativeAreal2DIdsAsync_Integration()
        {
            GISPostgreSQLConverterManager? gISPostgreSQLConverterManager = Create.GISPostgreSQLConverterManager();
            Assert.NotNull(gISPostgreSQLConverterManager);

            Building2DPostgreSQLConverter? building2DPostgreSQLConverter = gISPostgreSQLConverterManager.GetPostgreSQLConverter<Building2DPostgreSQLConverter>();
            Assert.NotNull(building2DPostgreSQLConverter);

            List<int> administrativeAreal2DIds = [73485, 78238];

            List<Building2DCentroid>? building2DCentroids = await building2DPostgreSQLConverter.GetBuilding2DCentroidsByAdministrativeAreal2DIdsAsync(administrativeAreal2DIds);
            Assert.NotNull(building2DCentroids);

            foreach (Building2DCentroid building2DCentroid in building2DCentroids)
            {
                Assert.False(string.IsNullOrWhiteSpace(building2DCentroid.Reference));
                Assert.NotNull(building2DCentroid.CountyId);
                Assert.False(double.IsNaN(building2DCentroid.X) || double.IsInfinity(building2DCentroid.X));
                Assert.False(double.IsNaN(building2DCentroid.Y) || double.IsInfinity(building2DCentroid.Y));
            }

            List<Building2DReference>? building2DReferences = await building2DPostgreSQLConverter.GetBuilding2DReferencesByAdministrativeAreal2DIdsAsync(administrativeAreal2DIds);
            Assert.NotNull(building2DReferences);

            HashSet<(string? Reference, int CountyId)> referenceSet = [];
            foreach (Building2DReference building2DReference in building2DReferences)
            {
                if (building2DReference.Reference is not null && building2DReference.CountyId is not null)
                {
                    referenceSet.Add((building2DReference.Reference, building2DReference.CountyId.Value));
                }
            }

            Assert.InRange(building2DCentroids.Count, 0, referenceSet.Count);

            foreach (Building2DCentroid building2DCentroid in building2DCentroids)
            {
                Assert.Contains((building2DCentroid.Reference, building2DCentroid.CountyId!.Value), referenceSet);
            }
        }
    }
}
