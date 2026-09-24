using DiGi.Analytical.Building.Interfaces;
using DiGi.Geometry.Spatial.Interfaces;
using System.Reflection;

namespace DiGi.Analytical.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests <see cref="Building.Query.Geometry3D{TGeometry3D}(IBuildingGeometry3DObject?)"/> on a component whose geometry converts to several results.
        /// <para>Guard for DiGi.Analytical#6: <c>MultiFaceComponent.json</c> is a roof with one hole taken from <c>BuildingModel_Envelope.json</c>. Asked for as <see cref="IPolygonal3D"/>, its face yields two rings (outer edge and hole), and the single-result query used to throw <see cref="NotImplementedException"/>.</para>
        /// </summary>
        [Fact]
        public void Geometry3D_MultiFaceComponent()
        {
            IComponent component = Geometry3D_MultiFaceComponent_Load();

            Assert.Null(Building.Query.Geometry3D<IPolygonal3D>(component));

            IPolygonalFace3D? polygonalFace3D = Building.Query.Geometry3D<IPolygonalFace3D>(component);
            Assert.NotNull(polygonalFace3D);
            Assert.Single(polygonalFace3D.InternalEdges!);

            List<IPolygonal3D>? polygonal3Ds = Building.Query.Geometry3Ds<IPolygonal3D>(component);
            Assert.NotNull(polygonal3Ds);
            Assert.Equal(2, polygonal3Ds.Count);
            Assert.Equal(polygonalFace3D.ExternalEdge!.GetPoints(), polygonal3Ds[0].GetPoints());
            Assert.Equal(polygonalFace3D.InternalEdges![0].GetPoints(), polygonal3Ds[1].GetPoints());

            Assert.Single(Building.Query.Geometry3Ds<IPolygonalFace3D>(component)!);
        }

        /// <summary>
        /// Tests that <see cref="Building.Query.Geometry3Ds{TGeometry3D}(IBuildingGeometry3DObject?)"/> returns null for a null object, and an empty list when the geometry cannot be represented as the requested type.
        /// </summary>
        [Fact]
        public void Geometry3Ds_NullAndUnconvertible()
        {
            Assert.Null(Building.Query.Geometry3Ds<IPolygonal3D>(null));
            Assert.Null(Building.Query.Geometry3D<IPolygonal3D>(null));

            List<Geometry.Spatial.Classes.Plane>? planes = Building.Query.Geometry3Ds<Geometry.Spatial.Classes.Plane>(Geometry3D_MultiFaceComponent_Load());
            Assert.NotNull(planes);
            Assert.Empty(planes);
        }

        private static IComponent Geometry3D_MultiFaceComponent_Load()
        {
            string? path = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "MultiFaceComponent.json");
            Assert.False(string.IsNullOrWhiteSpace(path));
            Assert.True(File.Exists(path));

            List<IComponent>? components = Core.Convert.ToDiGi<IComponent>((Core.Classes.Path)path);
            Assert.NotNull(components);

            return Assert.Single(components);
        }
    }
}
