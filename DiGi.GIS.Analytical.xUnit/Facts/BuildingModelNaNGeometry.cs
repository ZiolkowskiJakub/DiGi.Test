using DiGi.Analytical.Building;
using DiGi.Analytical.Building.Classes;
using DiGi.Analytical.Building.Interfaces;
using DiGi.Geometry.Spatial.Classes;
using DiGi.Geometry.Spatial.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace DiGi.GIS.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Loads the building models stored in the given file of the shared test files directory.
        /// </summary>
        /// <param name="fileName">The name of the file holding the serialized building models.</param>
        /// <returns>The building models, or <see langword="null"/> when the file is not available.</returns>
        private static List<BuildingModel>? BuildingModels_File(string fileName)
        {
            string? path = DiGi.Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), fileName);
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return null;
            }

            return DiGi.Core.Convert.ToDiGi<BuildingModel>((DiGi.Core.Classes.Path)path);
        }

        /// <summary>
        /// Collects the polygonal faces of the components of the given building models.
        /// </summary>
        /// <param name="buildingModels">The building models to be traversed.</param>
        /// <returns>The polygonal faces of every component carrying one.</returns>
        private static List<IPolygonalFace3D> PolygonalFace3Ds_Components(IEnumerable<BuildingModel> buildingModels)
        {
            List<IPolygonalFace3D> result = [];
            foreach (BuildingModel buildingModel in buildingModels)
            {
                List<IComponent>? components = buildingModel?.GetComponents<IComponent>();
                if (components is null)
                {
                    continue;
                }

                foreach (IComponent component in components)
                {
                    if (component.Surface3D() is IPolygonalFace3D polygonalFace3D)
                    {
                        result.Add(polygonalFace3D);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Determines whether the plane of the given face carries a not-a-number normal.
        /// </summary>
        /// <param name="polygonalFace3D">The face to be checked.</param>
        /// <returns><see langword="true"/> when the normal of the plane is not finite; otherwise, <see langword="false"/>.</returns>
        private static bool NaNNormal(IPolygonalFace3D polygonalFace3D)
        {
            DiGi.Geometry.Spatial.Classes.Vector3D? normal = polygonalFace3D?.Plane?.Normal;
            if (normal is null)
            {
                return false;
            }

            return double.IsNaN(normal.X) || double.IsNaN(normal.Y) || double.IsNaN(normal.Z);
        }

        /// <summary>
        /// Tests that building models carrying the corrupt geometry found in the database are converted without throwing.
        /// <para>Fixture captured from <c>gis/buildingmodel/itemsbycircle?x=629671.3&amp;y=489136.8&amp;radius=500</c> (county 55417). It holds eight building models whose walls and roofs were stored with a not-a-number plane normal - the plane origin is finite, so the corruption was introduced while deriving the normal, not by the source coordinates. The 2D geometry projected onto such a plane is entirely NaN.</para>
        /// <para>Regression guard for "System.ArgumentException: 'points must form a closed linestring'": NetTopologySuite tests a ring for closure by comparing its first and last coordinate, and NaN never equals NaN, so a correctly closed ring was rejected and the exception aborted the whole glTF scene instead of the single unusable component. Every affected face must now report itself as not triangulable, and no face may hand out a triangle built from NaN points.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_NaNGeometry_Triangulate()
        {
            List<BuildingModel>? buildingModels = BuildingModels_File("BuildingModel_NaNGeometry.json");
            if (buildingModels is null)
            {
                return;
            }

            Assert.Equal(8, buildingModels.Count);

            List<IPolygonalFace3D> polygonalFace3Ds = PolygonalFace3Ds_Components(buildingModels);
            Assert.NotEmpty(polygonalFace3Ds);

            int count_NaN = 0;
            foreach (IPolygonalFace3D polygonalFace3D in polygonalFace3Ds)
            {
                bool naN = NaNNormal(polygonalFace3D);
                if (naN)
                {
                    count_NaN++;
                }

                // Triangulate is the frame that threw. It must report the failure instead.
                List<Triangle3D>? triangle3Ds = polygonalFace3D.Triangulate(DiGi.Core.Constants.Tolerance.Distance);

                if (naN)
                {
                    Assert.True(triangle3Ds is null || triangle3Ds.Count == 0, "A face with a not-a-number plane normal was triangulated instead of being reported as not convertible.");
                    continue;
                }

                Assert.NotNull(triangle3Ds);
                foreach (Triangle3D triangle3D in triangle3Ds)
                {
                    foreach (Point3D? point3D in new Point3D?[] { triangle3D[0], triangle3D[1], triangle3D[2] })
                    {
                        Assert.False(point3D is null || double.IsNaN(point3D.X) || double.IsNaN(point3D.Y) || double.IsNaN(point3D.Z), "A triangle built from a not-a-number point was handed out.");
                    }
                }
            }

            // Pins the fixture: if a regenerated capture no longer carries the corruption this test stops
            // guarding anything, and that has to fail loudly rather than pass quietly.
            Assert.True(count_NaN > 0, "The fixture no longer carries any face with a not-a-number plane normal.");
        }

        /// <summary>
        /// Tests that building models holding sound geometry are still fully converted.
        /// <para>Positive control for <see cref="BuildingModel_NaNGeometry_Triangulate"/>, captured from the same response: the non-finite geometry guard must reject only the unusable faces and must never start discarding valid walls and roofs, which would silently punch holes into the rendered buildings.</para>
        /// </summary>
        [Fact]
        public void BuildingModel_ValidGeometry_Triangulate()
        {
            List<BuildingModel>? buildingModels = BuildingModels_File("BuildingModel_ValidGeometry.json");
            if (buildingModels is null)
            {
                return;
            }

            Assert.Equal(3, buildingModels.Count);

            List<IPolygonalFace3D> polygonalFace3Ds = PolygonalFace3Ds_Components(buildingModels);
            Assert.NotEmpty(polygonalFace3Ds);

            foreach (IPolygonalFace3D polygonalFace3D in polygonalFace3Ds)
            {
                Assert.False(NaNNormal(polygonalFace3D));

                List<Triangle3D>? triangle3Ds = polygonalFace3D.Triangulate(DiGi.Core.Constants.Tolerance.Distance);
                Assert.NotNull(triangle3Ds);
                Assert.NotEmpty(triangle3Ds);

                foreach (Triangle3D triangle3D in triangle3Ds)
                {
                    foreach (Point3D? point3D in new Point3D?[] { triangle3D[0], triangle3D[1], triangle3D[2] })
                    {
                        Assert.NotNull(point3D);
                        Assert.False(double.IsNaN(point3D.X) || double.IsNaN(point3D.Y) || double.IsNaN(point3D.Z));
                    }
                }
            }
        }
    }
}
