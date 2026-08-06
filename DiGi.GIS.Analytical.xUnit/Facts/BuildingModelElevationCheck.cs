using DiGi.Analytical.Building.Classes;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial.Classes;
using DiGi.GIS.Analytical.Enums;
using DiGi.GIS.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace DiGi.GIS.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Writes a report comparing the terrain elevation of the stored building models against the extents they were stored with.
        /// <para>This is a survey rather than an assertion - it exists to tell how far the models already in the database sit from the ground, which is what motivated threading a base elevation through the extrusion. The report lands in the reports directory, next to the ones written by the other surveys.</para>
        /// <para>Depends on the live GUGiK terrain service and produces an empty column for every building it cannot resolve. It does not fail when the service is unavailable, because it asserts nothing about the elevations themselves.</para>
        /// </summary>
        [Fact]
        public async Task BuildingModel_ElevationReportAsync()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            string? path = Core.xUnit.Query.FilePath(assembly, "BuildingModel_ValidGeometry.json");
            Assert.NotNull(path);
            Assert.True(File.Exists(path));

            List<BuildingModel>? buildingModels = Core.Convert.ToDiGi<BuildingModel>((Core.Classes.Path)path);
            Assert.NotNull(buildingModels);
            Assert.NotEmpty(buildingModels);

            using HttpClient httpClient = new();

            List<string> reportLines = ["Reference|Elevation|MinZ|MaxZ"];

            foreach (BuildingModel buildingModel in buildingModels)
            {
                Assert.NotNull(buildingModel);

                string reference = buildingModel.TryGetValue(BuildingModelParameter.Reference, out string? reference_Value) && !string.IsNullOrWhiteSpace(reference_Value)
                    ? reference_Value!
                    : buildingModel.Guid.ToString();

                BoundingBox3D? boundingBox3D = buildingModel.GetBoundingBox();
                Assert.NotNull(boundingBox3D);

                Point3D? centroid = boundingBox3D.GetCentroid();
                Assert.NotNull(centroid);

                Point3D? point3D_Elevation = await httpClient.ElevationAsync(new Point2D(centroid.X, centroid.Y));

                reportLines.Add($"{reference}|{point3D_Elevation?.Z}|{boundingBox3D.Min.Z}|{boundingBox3D.Max.Z}");
            }

            string? directory_Reports = Core.xUnit.Query.ReportsDirectory(assembly);
            Assert.False(string.IsNullOrWhiteSpace(directory_Reports));

            File.WriteAllLines(Path.Combine(directory_Reports!, "BuildingModel_Elevation_Report.txt"), reportLines);
        }

        /// <summary>
        /// Verifies that extruding a Building2D using BuildingModelAsync queries terrain elevation and sets the base elevation to the retrieved ground height.
        /// <para>Depends on the live GUGiK terrain service and will fail when it cannot be reached - the offline behaviour of the same creator is covered by <see cref="BuildingModelAsync_ElevationUnavailable"/>, which asserts that an unreachable service falls back to an elevation of zero rather than losing the building.</para>
        /// <para>The coordinate is a point in the PL-1992 grid whose terrain is well above ten metres, so the assertion distinguishes a resolved elevation from the zero the fallback would produce without pinning an exact ground height that the service may revise.</para>
        /// </summary>
        [Fact]
        public async Task BuildingModel_2DExtrusion_WithElevationAsync()
        {
            PolygonalFace2D? polygonalFace2D = Geometry.Planar.Create.PolygonalFace2D(
                new Point2D(489012, 630012),
                new Point2D(489022, 630012),
                new Point2D(489022, 630022),
                new Point2D(489012, 630022));
            Assert.NotNull(polygonalFace2D);

            Building2D building2D = new(Guid.NewGuid(), "REF123", polygonalFace2D, 2, null, null, []);

            using HttpClient httpClient = new();
            BuildingModel? buildingModel = await httpClient.BuildingModelAsync(building2D, storeyHeight: 3.0);

            Assert.NotNull(buildingModel);
            BoundingBox3D? boundingBox3D = buildingModel.GetBoundingBox();
            Assert.NotNull(boundingBox3D);
            Assert.True(boundingBox3D.Min.Z > 10.0, "Building base elevation should be at ground terrain height");
        }
    }
}
