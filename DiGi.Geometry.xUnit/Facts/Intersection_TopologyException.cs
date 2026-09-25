using DiGi.Geometry.Planar;
using DiGi.Geometry.Planar.Classes;
using System.Reflection;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Query.Intersection(PolygonalFace2D?, PolygonalFace2D?)"/> recovers with its <c>OverlayNGRobust</c> retry when NetTopologySuite's overlay throws a <c>TopologyException</c> on valid input, instead of returning <c>null</c> (ZiolkowskiJakub/DiGi.Geometry#9).
        /// <para>The fixture <c>IntersectionTopologyException.json</c> holds, in this order, a 31.787 m² receiver face and the 22 shadows that cover it: a gable wall of building 11043814 (county 55417) fully hidden by the adjacent building, lit from one sky patch (ZiolkowskiJakub/DiGi.GIS.WebAPI.UI#61).
        /// Their union is one valid 107.5 m² face with zero-area sliver holes, and the classic overlay of that face with the receiver throws "found non-noded intersection". The fact first proves the fixture still triggers the exception, then that the intersection covers the receiver except for about 0.02 m².</para>
        /// </summary>
        [Fact]
        public void Intersection_TopologyException()
        {
            string? path = DiGi.Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "IntersectionTopologyException.json");
            Assert.False(string.IsNullOrWhiteSpace(path));

            List<PolygonalFace2D>? polygonalFace2Ds = DiGi.Core.Convert.ToDiGi<PolygonalFace2D>((DiGi.Core.Classes.Path)path);
            Assert.NotNull(polygonalFace2Ds);
            Assert.Equal(23, polygonalFace2Ds.Count);

            PolygonalFace2D polygonalFace2D_Receiver = polygonalFace2Ds[0];
            List<PolygonalFace2D> polygonalFace2Ds_Shadow = polygonalFace2Ds.GetRange(1, 22);
            double area_Receiver = polygonalFace2D_Receiver.GetArea();
            Assert.Equal(31.787, area_Receiver, 3);

            List<PolygonalFace2D>? polygonalFace2Ds_Union = polygonalFace2Ds_Shadow.Union();
            Assert.NotNull(polygonalFace2Ds_Union);
            PolygonalFace2D polygonalFace2D_Union = Assert.Single(polygonalFace2Ds_Union);

            NetTopologySuite.Geometries.Polygon? polygon_Union = polygonalFace2D_Union.ToNTS();
            NetTopologySuite.Geometries.Polygon? polygon_Receiver = polygonalFace2D_Receiver.ToNTS();
            Assert.NotNull(polygon_Union);
            Assert.NotNull(polygon_Receiver);
            Assert.True(polygon_Union.IsValid);
            Assert.True(polygon_Receiver.IsValid);
            Assert.Throws<NetTopologySuite.Geometries.TopologyException>(() => polygon_Union.Intersection(polygon_Receiver));

            List<PolygonalFace2D>? polygonalFace2Ds_Intersection = polygonalFace2D_Union.Intersection(polygonalFace2D_Receiver);
            Assert.NotNull(polygonalFace2Ds_Intersection);

            double area_Intersection = polygonalFace2Ds_Intersection.Sum(x => x.GetArea());
            Assert.True(area_Intersection <= area_Receiver + 1e-6, $"intersection {area_Intersection} above receiver {area_Receiver}");
            Assert.True(area_Intersection > area_Receiver - 0.05, $"intersection {area_Intersection} of receiver {area_Receiver}");
        }
    }
}
