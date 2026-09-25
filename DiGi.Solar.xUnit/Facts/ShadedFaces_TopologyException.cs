using DiGi.Geometry.Planar.Classes;
using System.Reflection;

namespace DiGi.Solar.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that a receiver fully covered by shadows whose merged face NetTopologySuite fails to clip reads as shaded, not as sunlit (ZiolkowskiJakub/DiGi.Solar#16).
        /// <para>The fixture <c>IntersectionTopologyException.json</c> holds a 31.787 m² receiver (a gable wall of building 11043814 hidden by the adjacent building, ZiolkowskiJakub/DiGi.GIS.WebAPI.UI#61) and the 22 shadows of one sky patch, recorded from <see cref="Query.ProjectedShadowFaces(Geometry.Spatial.Classes.Plane?, BoundingBox2D?, double[], int[], int, Geometry.Spatial.Classes.Vector3D?, double)"/>. Their union has zero-area sliver holes on which the classic overlay throws.
        /// Before the fix <see cref="Query.ShadedFaces(PolygonalFace2D?, IEnumerable{PolygonalFace2D}?)"/> dropped the failed clip and reported 0 m². Since ZiolkowskiJakub/DiGi.Geometry#9 the clip succeeds and reports 31.77 m²; with the clip failing (measured against the pre-#9 Geometry build) the unmerged fallback reports 25.47 m² - it stops adding shadows before their summed area breaks the receiver cap - so the bound, 75 % of the receiver, holds on both paths and 0 fails it.</para>
        /// </summary>
        [Fact]
        public void ShadedFaces_TopologyException()
        {
            string? path = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "IntersectionTopologyException.json");
            Assert.False(string.IsNullOrWhiteSpace(path));

            List<PolygonalFace2D>? polygonalFace2Ds = Core.Convert.ToDiGi<PolygonalFace2D>((Core.Classes.Path)path);
            Assert.NotNull(polygonalFace2Ds);
            Assert.Equal(23, polygonalFace2Ds.Count);

            PolygonalFace2D polygonalFace2D_Receiver = polygonalFace2Ds[0];
            double area_Receiver = polygonalFace2D_Receiver.GetArea();

            List<PolygonalFace2D>? polygonalFace2Ds_Shaded = Query.ShadedFaces(polygonalFace2D_Receiver, polygonalFace2Ds.GetRange(1, 22));
            Assert.NotNull(polygonalFace2Ds_Shaded);

            double area_Shaded = polygonalFace2Ds_Shaded.Sum(x => x.GetArea());
            Assert.True(area_Shaded <= area_Receiver + 1e-6, $"shaded {area_Shaded} above receiver {area_Receiver}");
            Assert.True(area_Shaded > area_Receiver * 0.75, $"shaded {area_Shaded} of receiver {area_Receiver}");
        }
    }
}
