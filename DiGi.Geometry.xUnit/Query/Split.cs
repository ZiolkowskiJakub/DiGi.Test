using DiGi.Geometry.Spatial;
using DiGi.Geometry.Spatial.Classes;

namespace DiGi.Geometry.xUnit
{
    public partial class Query
    {
        [Fact]
        public void Split()
        {
            PolygonalFace3D? polygonalFace3D = Create.PolygonalFace3D(Spatial.Constans.Plane.WorldZ, new Planar.Classes.Point2D(0, 0), new Planar.Classes.Point2D(0, 10), new Planar.Classes.Point2D(10, 10), new Planar.Classes.Point2D(10, 0));

            Assert.NotNull(polygonalFace3D);

            if (polygonalFace3D is null)
            {
                return;
            }

            PolygonalFaceExtrusion polygonalFaceExtrusion = new(polygonalFace3D, Spatial.Constans.Vector3D.WorldZ * 10);

            Assert.NotNull(polygonalFaceExtrusion);

            if (polygonalFaceExtrusion is null)
            {
                return;
            }

            Polyhedron? polyhedron = Create.Polyhedron(polygonalFaceExtrusion);
            Assert.NotNull(polyhedron);

            Assert.True(Spatial.Query.TrySplit(new Plane(Create.Plane(1.0)), polyhedron, out List<Polyhedron>? polyhedrons));

            Assert.NotNull(polyhedrons);
            if(polyhedrons is null)
            {
                return;
            }

            foreach(Polyhedron polyhedron_Split in polyhedrons)
            {
                Point3D? internalPoint = polyhedron_Split?.GetInternalPoint();
                Assert.NotNull(internalPoint);

                if(internalPoint is null)
                {
                    continue;
                }

                bool inside = polyhedron.Inside(internalPoint);
                Assert.True(inside);
            }
        }
    }
}